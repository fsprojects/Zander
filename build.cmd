@echo off
cls

dotnet restore build.proj

IF NOT EXIST build.fsx (
  dotnet fake run init.fsx
)
dotnet fake build %*