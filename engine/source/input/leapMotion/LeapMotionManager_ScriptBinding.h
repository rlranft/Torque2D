//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

ConsoleFunction(initLeapMotionManager, void, 1, 1, "() Initialize the LeapMotionManager")
{
    if (gLeapMotionManager != NULL)
    {
        Con::printf("LeapMotionManager already initialized");
    }
    else
    {
        gLeapMotionManager = new LeapMotionManager();
    }
}

//-----------------------------------------------------------------------------

ConsoleFunction(enableLeapMotionManager, void, 2, 2, "(bool enabledState) Run or pause the LeapMotionManager.\n"
                                                     "@param enabledState True to turn it on, false otherwise")
{
    if (gLeapMotionManager == NULL)
    {
        Con::printf("LeapMotionManager not initialized. Call initLeapMotionManager() first");
    }
    else
    {
        gLeapMotionManager->enable(dAtob(argv[1]));
    }
}

//-----------------------------------------------------------------------------

ConsoleFunction(isLeapMotionManagerEnabled, bool, 1, 1, "() Checks the LeapMotionManager to see if it is enabled.\n"
                                                        "@return True if it's running, false otherwise")
{
    if (gLeapMotionManager == NULL)
    {
        Con::printf("LeapMotionManager not initialized. Call initLeapMotionManager() first");
        return false;
    }
    else
    {
        return gLeapMotionManager->getEnabled();
    }
}

//-----------------------------------------------------------------------------

ConsoleFunction(enableLeapCursorControl, void, 2, 2, "(bool enabledState) Toggles the manager to act like a mouse.\n"
                                                        "@param enabledState True to act like a mouse, false otherwise")
{
    if (gLeapMotionManager == NULL)
    {
        Con::printf("LeapMotionManager not initialized. Call initLeapMotionManager() first");
    }
    else
    {
        return gLeapMotionManager->toggleMouseControl(dAtob(argv[1]));
    }
}

//-----------------------------------------------------------------------------

ConsoleFunction(isLeapCursorControlled, bool, 1, 1, "() Checks the LeapMotionManager to see if it is controlling the mouse.\n"
        "@return True if it's acting as a mouse, false otherwise")
{
    if (gLeapMotionManager == NULL)
    {
        Con::printf("LeapMotionManager not initialized. Call initLeapMotionManager() first");
        return false;
    }
    else
    {
        return gLeapMotionManager->getMouseControlToggle();
    }
}

//-----------------------------------------------------------------------------

ConsoleFunction(configureLeapGesture, bool, 3, 3,   "(gestureString, value) Modified a Config string on the main "
                                                    "Controller, via the LeapMotionManager. The following strings are allowed:\n"
                                                    "Gesture.Circle.MinProgress\n"
                                                    "Gesture.Circle.MinRadius\n"
                                                    "Gesture.Circle.MinArc\n"
                                                    "Gesture.Swipe.MinLength\n"
                                                    "Gesture.Swipe.MinVelocity\n"
                                                    "Gesture.KeyTap.MinDownVelocity\n"
                                                    "Gesture.KeyTap.HistorySeconds\n"
                                                    "Gesture.KeyTap.MinDistance\n"
                                                    "Gesture.ScreenTap.MinForwardVelocity\n"
                                                    "Gesture.ScreenTap.HistorySeconds\n"
                                                    "Gesture.ScreenTap.MinDistance\n"
                                                    "@param gestureString The Config string to be set\n"
                                                    "@param value The new value for the Config string\n"
                                                    "@return True if string was successfully set, false otherwise")
{
    if (gLeapMotionManager == NULL)
    {
        Con::printf("LeapMotionManager not initialized. Call initLeapMotionManager() first");
        return false;
    }
    else
    {
        if (!dStrcmp("Gesture.Circle.MinProgress", argv[1]))
        {
            return gLeapMotionManager->setMinCircleProgress(dAtof(argv[2]));
        }
        else
        {
            return gLeapMotionManager->configureLeapGesture(argv[1], dAtof(argv[2]));
        }
    }
}