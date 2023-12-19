@echo off
setlocal

call TestBuild.bat BuildTest%1.csv
call TestInserts.bat InsertTest%1.csv
