set UNITY_VERSION=2020.3.17f1
 
c:
set JAVA_HOME=c:\Unity\%UNITY_VERSION%\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\
set ANDROID_HOME=c:\Unity\%UNITY_VERSION%\Editor\Data\PlaybackEngines\AndroidPlayer\
cd %ANDROID_HOME%SDK\tools\bin\
echo.> %USERPROFILE%\.android\repositories.cfg
 
cmd /C sdkmanager --update
cmd /C sdkmanager "platform-tools" "platforms;android-29"
cmd /C sdkmanager "platform-tools" "platforms;android-30"
cmd /C sdkmanager "platform-tools" "platforms;android-31"