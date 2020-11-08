#r "paket:
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Target
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

let buildDir = "./build/"

let projectName = "CRMSvcUtilExtensions"
let authors = ["Sebastian Holager"]
let description = "A library with extensions to CRMSvcUtil"
let release = ReleaseNotes.load "RELEASE_NOTES.md"
let apiKey = Environment.environVarOrDefault "apiKey" " "

Target.create "Clean" (fun _ ->
  Shell.cleanDir buildDir
  Environment.setEnvironVar "Version" release.NugetVersion
)

Target.create "Build" (fun _ ->
    AssemblyInfoFile.createCSharp "/src/CRMSvcUtilExtensions/Properties/AssemblyInfo.cs"
        [AssemblyInfo.Title projectName
         AssemblyInfo.Description description
         AssemblyInfo.Product projectName
         AssemblyInfo.Guid "76e6ae49-230a-472b-a132-e8ad2f821e64"
         AssemblyInfo.Version release.AssemblyVersion
         AssemblyInfo.FileVersion release.AssemblyVersion]

    !! "./**/*.csproj"
    |> MSBuild.runRelease id buildDir "Build"
    |> Trace.logItems "AppBuild-Output: "
)

Target.create "Package" (fun _ ->
    let packageDir = "./packaging/"
    let net472Dir = packageDir @@ "lib/net472/"
    Shell.cleanDirs [packageDir;net472Dir]
    let dependencies = NuGet.getDependencies "./src/CRMSvcUtilExtensions/packages.config"
    
    Shell.copyFile net472Dir (buildDir @@ "CRMSvcUtilExtensions.dll")

    NuGet.NuGet (fun p ->
        {p with
             Project = projectName
             Authors = authors
             Version = release.NugetVersion
             OutputPath = packageDir
             WorkingDir = packageDir
             ReleaseNotes = release.Notes |> String.toLines
             Dependencies = dependencies
             Description = description
             AccessKey = apiKey
             Publish = false 
             PublishUrl = "https://www.nuget.org/api/v2/package" })
        "./src/CRMSvcUtilExtensions/CRMSvcUtilExtensions.nuspec"
)

open Fake.Core.TargetOperators

// Dependencies
"Clean"
  ==> "Build"
  ==> "Package"
  
Target.runOrDefault "Package"