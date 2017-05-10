cd src
dotnet restore Itemify Itemify.Core Itemify.PostgreSql Itemify.Shared
dotnet pack Itemify 
dotnet pack Itemify.Core 
dotnet pack Itemify.PostgreSql 
dotnet pack Itemify.Shared
::move /Y Itemify.Shared\bin\Debug\Itemify.Shared.*.0.nupkg test
::move /Y Itemify.PostgreSql\bin\Debug\Itemify.PostgreSql.*.0.nupkg test
::move /Y Itemify.Core\bin\Debug\*.0.nupkg test
move /Y Itemify\bin\Debug\*.0.nupkg C:\nuget\
cd ..

pause