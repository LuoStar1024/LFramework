#!/bin/bash

[ -d Luban ] && rm -rf LubanDll

dotnet build  ./LubanCode/Luban/Luban.csproj -c Release -o LubanDll