@echo off
setlocal

set STARTTIME=%TIME%
echo STARTTIME: %STARTTIME%
set /A STARTTIME=(100 + %STARTTIME:~0,2%-100)*360000 + (1%STARTTIME:~3,2%-100)*6000 + (1%STARTTIME:~6,2%-100)*100 + (1%STARTTIME:~9,2%-100)

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 100000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 500000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 1000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 2000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 3000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 4000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 5000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 6000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 10000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 15000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%

"C:\Code\Git\Various\MyConcurrentDictionary\MyConcurrentDictionary\bin\Release\MyConcurrentDictionary.exe" 20000000 "C:\Code\Varonis Assignment\CSVs\%1"

set ENDTIME=%TIME%
set /A ENDTIME=(100 + %ENDTIME:~0,2%-100)*360000 + (1%ENDTIME:~3,2%-100)*6000 + (1%ENDTIME:~6,2%-100)*100 + (1%ENDTIME:~9,2%-100)

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%
rem now break the centiseconds down to hours, minutes, seconds and the remaining centiseconds
set /A DURATIONH=%DURATION% / 360000
set /A DURATIONM=(%DURATION% - %DURATIONH%*360000) / 6000
set /A DURATIONS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000) / 100
set /A DURATIONHS=(%DURATION% - %DURATIONH%*360000 - %DURATIONM%*6000 - %DURATIONS%*100)

rem some formatting
if %DURATIONH% LSS 10 set DURATIONH=0%DURATIONH%
if %DURATIONM% LSS 10 set DURATIONM=0%DURATIONM%
if %DURATIONS% LSS 10 set DURATIONS=0%DURATIONS%
if %DURATIONHS% LSS 10 set DURATIONHS=0%DURATIONHS%

rem outputing
echo TIME: %TIME%
echo DURATION: %DURATIONH%:%DURATIONM%:%DURATIONS%.%DURATIONHS%
