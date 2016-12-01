# Smart Trade

## What

Android application that buys or sells on bitstamp.net with a unit cost averaging strategy.


## Status

**Very** early development stage, nothing works yet.


## Development Setup

1. Install [node.js](https://nodejs.org/en/download/).
2. Download and install [Java SE Development Kit](http://www.oracle.com/technetwork/java/javase/downloads/index.html).
3. Download the **Android SDK**, which can be found at the **bottom** of the
   [Android Studio Page](https://developer.android.com/studio/index.html).
4. Install the **Android SDK** with default settings and let the installer run the **SDK Manager** at the end.
5. In the **SDK Manager**, make sure at least the following packages are selected to be installed:
   **Android SDK Tools**, **Android Support Repository** and **Google USB Driver**.
6. Install the selected packages and close the **SDK Manager**.
7. [Set the required environment variables](https://cordova.apache.org/docs/en/latest/guide/platforms/android/index.html#setting-environment-variables)
   (`JAVA_HOME`and `ANDROID_HOME`) and log off and log on again (so that the environment variables become active).
8. Pull this repository and open a new command line with the current working directory set to the root folder.
9. Execute `npm install`. Unless you've chosen to install more packages than the ones mentioned in step 5, this step
   will fail with a list of additional packages that need to be installed.
10. Start **SDK Manager** as Administrator, verify the packages mentioned in step 9 and check the ones that are not yet
    installed.
11. Install the packages.
12. Back on the command line, execute `npm install` again and make sure there are no error messages.
13. Connect an **Android** device via USB and enable debug mode.
14. Enter `npm start` on the command line. This should start the application on the connected device.
