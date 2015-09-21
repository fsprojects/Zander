require 'albacore'
require 'nuget_helper'
task :default => [:build]
dir = File.dirname(__FILE__)

desc "build solution"
build :build do |msb, args|
  msb.prop :configuration, :Debug
  msb.target = [:Rebuild]
  msb.sln = "Zander.sln"
end

desc "test using console"
test_runner :test => [:build] do |runner|
  runner.exe = NugetHelper::nunit_path
  files = Dir.glob(File.join(dir, "**", "bin", "Debug", "*Tests.dll"))
  runner.files = files 
end

