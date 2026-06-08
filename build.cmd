@echo off
cls

dotnet tool restore
dotnet paket restore

IF NOT EXIST build.fsx (
  dotnet fake run init.fsx
)
dotnet fake build %*