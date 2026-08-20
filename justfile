set windows-shell := ["powershell.exe"]

default:
  @just --list

build:
  dotnet build

run:
  dotnet run

watch:
  dotnet watch run

clean:
  dotnet clean
  rm -rf bin obj

restore:
  dotnet restore

format:
  dotnet format
