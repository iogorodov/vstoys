using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using VSToys.Utils;

namespace VSToys
{
	public sealed class VSToysSettings : XmlSerializer
	{
		public enum Scope
		{
			Application = 0,
			Solution = 1,
		}
		
		public interface IConfigObject
		{
			void OnBeforeSerialize( VSToysPackage package, Scope scope );
			void OnAfterDeserialize( VSToysPackage package, Scope scope );
		}

		public sealed class SettingsAttribute : Attribute
		{
			private readonly Scope scope = Scope.Application;

			public SettingsAttribute( Scope scope ) { this.scope = scope; }

			public Scope Scope { get { return scope; } }
		}

		private readonly static XmlReaderSettings readerSettings = new XmlReaderSettings { CloseInput = false, ConformanceLevel = ConformanceLevel.Document, IgnoreWhitespace = true, IgnoreComments = true };
		private readonly static XmlWriterSettings writerSettings = new XmlWriterSettings { CloseOutput = false, ConformanceLevel = ConformanceLevel.Document, Encoding = Encoding.UTF8, Indent = true, IndentChars = "\t" };

		private readonly Dictionary<string, Type> knownTypes = new Dictionary<string, Type>();
		private readonly Dictionary<string, IConfigObject> objects = new Dictionary<string, IConfigObject>();

		private Scope activeScope = Scope.Application;

		public VSToysSettings()
		{
		}

		protected override Type TryGetType( string typeName )
		{
			Type result = null;
			if ( !knownTypes.TryGetValue( typeName, out result ) )
				return null;

			return result;
		}

		protected override IEnumerable<FieldInfo> GetFields( Type type, bool serialize )
		{
			List<FieldInfo> result = new List<FieldInfo>();
			foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
			{
				if ( TypeUtils.HasAttribute<SettingsAttribute>( field, true ) && TypeUtils.GetAttribute<SettingsAttribute>( field, true ).Scope == activeScope )
					result.Add( field );
			}

			return result;
		}

		protected override IEnumerable<PropertyInfo> GetProperties( Type type, bool serialize )
		{
			return new List<PropertyInfo>();
		}

		public void RegisterObject( IConfigObject obj )
		{
			knownTypes.Add( obj.GetType().Name, obj.GetType() );
			foreach ( Type nestedType in obj.GetType().GetNestedTypes( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic ) )
				knownTypes.Add( nestedType.Name, nestedType );

			objects.Add( obj.GetType().Name, obj );
		}

		private void LoadSettings( VSToysPackage package, Stream stream, Scope scope )
		{
      try
      {
        activeScope = scope;
        using ( XmlReader reader = XmlReader.Create( stream, readerSettings ) )
        {
          if ( !reader.IsStartElement() || reader.Name != GetType().Name )
            return;

          reader.ReadStartElement();
          while ( reader.IsStartElement() )
          {
            IConfigObject obj = null;
            if ( !objects.TryGetValue( reader.Name, out obj ) )
            {
              XmlUtils.SkipNode( reader );
              continue;
            }
            Deserialize( obj, reader );
            obj.OnAfterDeserialize( package, activeScope );
          }
          reader.ReadEndElement();
        }
      }
      catch { }
		}

		private void StoreSettings( VSToysPackage package, Stream stream, Scope scope )
		{
      try
      {
        activeScope = scope;
        using ( XmlWriter writer = XmlWriter.Create( stream, writerSettings ) )
        {
          writer.WriteStartElement( GetType().Name );
          List<string> names = new List<string>( objects.Keys );
          names.Sort( string.Compare );
          foreach ( string name in names )
          {
            IConfigObject obj = null;
            if ( !objects.TryGetValue( name, out obj ) )
              continue;
            obj.OnBeforeSerialize( package, activeScope );
            Serialize( obj, writer );
          }
          writer.WriteEndElement();
          writer.Flush();
        }
      }
      catch { }
		}

		public void LoadApplicationSettings( VSToysPackage package )
		{
			string userSettingsFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), package.GetType().Name );
			string userSettingsFile = Path.Combine( userSettingsFolder, GetType().Name ) + ".settings";
			if ( !File.Exists( userSettingsFile ) )
				return;

			using ( Stream stream = new FileStream( userSettingsFile, FileMode.Open, FileAccess.Read, FileShare.Read ) )
				LoadSettings( package, stream, Scope.Application );
		}

		public void LoadSolutionSettings( VSToysPackage package, Stream stream )
		{
			LoadSettings( package, stream, Scope.Solution );
		}

		public void StoreSolutionSettings( VSToysPackage package, Stream stream )
		{
			using ( MemoryStream buffer = new MemoryStream() )
			{
				StoreSettings( package, buffer, Scope.Solution );
				buffer.Seek( 0, SeekOrigin.Begin );
				stream.Write( buffer.GetBuffer(), 0, (int)buffer.Length );
				stream.Flush();
			}
		}

		public void StoreApplicationSettings( VSToysPackage package )
		{
			string userSettingsFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), package.GetType().Name );
			string userSettingsFile = Path.Combine( userSettingsFolder, GetType().Name ) + ".settings";
			if ( !Directory.Exists( userSettingsFolder ) )
				Directory.CreateDirectory( userSettingsFolder );

			using ( Stream stream = new FileStream( userSettingsFile, FileMode.Create, FileAccess.Write ) )
			{
				StoreSettings( package, stream, Scope.Application );
				stream.Flush();
			}
		}
	}
}
