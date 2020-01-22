using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FrostyEditor;
using FrostySdk.Attributes;
using FrostySdk.Managers;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: ResCustomHandler(ResourceType.ShaderBlockDepot, typeof(ShaderBlockDepotCustomActionHandler))]
[assembly: AssemblyTitle("FrostySoundImport")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FrostySoundImport")]
[assembly: AssemblyCopyright("Copyright DanDev ©  2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: System.Runtime.InteropServices.Guid("9b4aeb1e-94de-42da-ae1d-af1fc27d9902")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]
