# Smart Trade

## What

Android application that buys or sells on bitstamp.net with a unit cost averaging strategy.


## Status

**Very** early development stage, nothing works yet.


## Development Setup

1. Install [node.js](https://nodejs.org/en/download/).
2. Follow
   [Installing the Requirements]("https://cordova.apache.org/docs/en/latest/guide/platforms/android/index.html#installing-the-requirements")
   to install the Android prerequisites. See step 3 on how to install the **Android SDK**.
3. Download the **SDK tools package**, which can be found at the **bottom** of the
   [Android Studio Page](https://developer.android.com/studio/index.html).
4. Install the SDK tools package with default settings and let the installer run the **SDK Manager** at the end.
5. Make sure at least the following packages are selected to be installed: **Android SDK Tools**,
   **Android Support Repository** and **Google USB Driver**.
6. Install the selected packages and leave the **SDK Manager** open.
7. Pull this repository and open a new command line with the current working directory set to the root folder.
8. Execute `npm install` in the root directory. Unless you've chosen to install more packages than
   the ones mentioned in step 5, this step will fail with a list of additional packages that need to be installed.
9. Back in the **SDK Manager**, select at least the mentioned packages and install them.
10. Back on the command line, execute `npm install` again and make sure there are no error messages.
11. Connect an Android device via USB.
12. Enter `npm start` on the command line. This should start the application on the connected device.
