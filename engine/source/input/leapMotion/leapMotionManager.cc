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

#ifndef _LEAPMOTIONMANAGER_H_
#include "input/leapMotion/leapMotionManager.h"
#endif

#ifndef _CONSOLETYPES_H_
#include "console/consoleTypes.h"
#endif

#ifndef _EVENT_H_
#include "platform/event.h"
#endif

#ifndef _GAMEINTERFACE_H_
#include "game/gameInterface.h"
#endif

#ifndef _GUICANVAS_H_
#include "gui/guiCanvas.h"
#endif

#ifndef _LEAPMOTIONUTIL_H_
#include "leapMotionUtil.h"
#endif

#include "input/leapMotion/LeapMotionManager_ScriptBinding.h"

//-----------------------------------------------------------------------------

bool LeapMotionManager::smEnableDevice = true;

bool LeapMotionManager::smGenerateIndividualEvents = true;
bool LeapMotionManager::smKeepHandIndexPersistent = false;
bool LeapMotionManager::smKeepPointableIndexPersistent = false;

bool LeapMotionManager::smGenerateSingleHandRotationAsAxisEvents = false;

F32 LeapMotionManager::smMaximumHandAxisAngle = 25.0f;

bool LeapMotionManager::smGenerateWholeFrameEvents = false;

U32 LeapMotionManager::LM_FRAMEVALIDDATA = 0;
U32 LeapMotionManager::LM_HAND[LeapMotionConstants::MaxHands] = {0};
U32 LeapMotionManager::LM_HANDROT[LeapMotionConstants::MaxHands] = {0};
U32 LeapMotionManager::LM_HANDAXISX = 0;
U32 LeapMotionManager::LM_HANDAXISY = 0;
U32 LeapMotionManager::LM_HANDPOINTABLE[LeapMotionConstants::MaxHands][LeapMotionConstants::MaxPointablesPerHand] = {0};
U32 LeapMotionManager::LM_HANDPOINTABLEROT[LeapMotionConstants::MaxHands][LeapMotionConstants::MaxPointablesPerHand] = {0};
U32 LeapMotionManager::LM_FRAME = 0;

//-----------------------------------------------------------------------------

LeapMotionManager::LeapMotionManager()
{
    // Initialize the console variables
    staticInit();

    // Create our controller and listener
    mListener = new MotionListener();
    mController = new Leap::Controller();
    mController->addListener(*mListener);

    // Allocate a mutex to use later
    mActiveMutex = Mutex::createMutex();

    // Nothing is ready yet
    mEnabled = false;
    mActive = false;
}

//-----------------------------------------------------------------------------

LeapMotionManager::~LeapMotionManager()
{
    // Disable and delete internal members
    disable();

    // Get rid of the mutex
    Mutex::destroyMutex(mActiveMutex);
}

//-----------------------------------------------------------------------------

void LeapMotionManager::staticInit()
{
    // If true, the Leap Motion device will be enabled, if present
    Con::addVariable("pref::LeapMotion::EnableDevice", TypeBool, &smEnableDevice);
   
    // Indicates that events for each hand and pointable will be created.
    Con::addVariable("LeapMotion::GenerateIndividualEvents", TypeBool, &smGenerateIndividualEvents);
      
	// Indicates that we track hand IDs and will ensure that the same hand will remain at the same index between frames.   
    Con::addVariable("LeapMotion::KeepHandIndexPersistent", TypeBool, &smKeepHandIndexPersistent);
    
	// Indicates that we track pointable IDs and will ensure that the same pointable will remain at the same index between frames.   
    Con::addVariable("LeapMotion::KeepPointableIndexPersistent", TypeBool, &smKeepPointableIndexPersistent);
    
	// If true, broadcast single hand rotation as axis events.
    Con::addVariable("LeapMotion::GenerateSingleHandRotationAsAxisEvents", TypeBool, &smGenerateSingleHandRotationAsAxisEvents);
    
	// The maximum hand angle when used as an axis event as measured from a vector pointing straight up (in degrees).
    // Shoud range from 0 to 90 degrees.   
    Con::addVariable("LeapMotion::MaximumHandAxisAngle", TypeF32, &smMaximumHandAxisAngle);
    
    // Indicates that a whole frame event should be generated and frames should be buffered.
    Con::addVariable("LeapMotion::GenerateWholeFrameEvents", TypeBool, &smGenerateWholeFrameEvents);   
}

//-----------------------------------------------------------------------------

void LeapMotionManager::enable(bool enabledState)
{
    Mutex::lockMutex(mActiveMutex);
    mEnabled = enabledState;
    Mutex::unlockMutex(mActiveMutex);
}

//-----------------------------------------------------------------------------

void LeapMotionManager::disable()
{
    if (mController)
    {
        delete mController;
        mController = NULL;

        if (mListener)
        {
            delete mListener;
            mListener = NULL;
        }
    }

    setActive(false);
    mEnabled = false;
}

//-----------------------------------------------------------------------------

bool LeapMotionManager::getActive()
{
    Mutex::lockMutex(mActiveMutex);
    bool active = mActive;
    Mutex::unlockMutex(mActiveMutex);

    return active;
}

//-----------------------------------------------------------------------------

void LeapMotionManager::setActive(bool state)
{
    Mutex::lockMutex(mActiveMutex);
    mActive = state;
    Mutex::unlockMutex(mActiveMutex);
}

//-----------------------------------------------------------------------------

void LeapMotionManager::toggleMouseControl(bool enabledState)
{
    Mutex::lockMutex(mActiveMutex);
    mMouseControl = enabledState;
    Mutex::unlockMutex(mActiveMutex);
}

//-----------------------------------------------------------------------------

bool LeapMotionManager::getMouseControlToggle()
{
    Mutex::lockMutex(mActiveMutex);
    bool mouseControlled = mMouseControl;
    Mutex::unlockMutex(mActiveMutex);

    return mouseControlled;
}

//-----------------------------------------------------------------------------

void LeapMotionManager::process(const Leap::Controller& controller)
{
    // Is the manager enabled?
    if (!mEnabled)
        return;

    // Was the leap device activated
    if (!getActive())
        return;

    // Get the current frame
    const Leap::Frame frame = controller.frame();

    if (!frame.isValid())
        return;

    // Get gestures
    const Leap::GestureList gestures = frame.gestures();

    for (int g = 0; g < gestures.count(); ++g)
    {
        Leap::Gesture gesture = gestures[g];

        switch (gesture.type())
        {
            case Leap::Gesture::TYPE_CIRCLE:
            {
                Leap::CircleGesture circle = gesture;
                bool clockWise;

                if (circle.pointable().direction().angleTo(circle.normal()) <= Leap::PI/4) 
                    clockWise = true;
                else 
                    clockWise = false;
            
                // Calculate angle swept since last frame
                float sweptAngle = 0;
                
                if (circle.state() != Leap::Gesture::STATE_START) 
                {
                    Leap::CircleGesture previousUpdate = Leap::CircleGesture(controller.frame(1).gesture(circle.id()));
                    sweptAngle = (circle.progress() - previousUpdate.progress()) * 2 * Leap::PI;
                }
                
                InputEvent event;
            
                event.deviceInst = 0;
                event.iValue = g;
                event.fValues[0] = (F32)circle.state();;
                event.fValues[1] = circle.radius();
                event.fValues[2] = clockWise;
                event.deviceType = LeapMotionDeviceType;
                event.objType = SI_CIRCLE_GESTURE;
                event.objInst = 0;
                event.action = SI_GESTURE;
                event.modifier = 0;
            
                Game->postEvent(event);
                break;
            }
            case Leap::Gesture::TYPE_SWIPE:
            {
                Leap::SwipeGesture swipe = gesture;
                
                InputEvent event;
            
                event.deviceInst = 0;
                event.iValue = g;
                event.fValues[0] = (F32)swipe.state();
                event.fValues[1] = swipe.direction().x;
                event.fValues[2] = swipe.direction().y;
                event.fValues[3] = swipe.direction().z;
                event.fValues[4] = swipe.speed();
                event.deviceType = LeapMotionDeviceType;
                event.objType = SI_SWIPE_GESTURE;
                event.objInst = 0;
                event.action = SI_GESTURE;
                event.modifier = 0;
            
                Game->postEvent(event);
                break;
            }
            case Leap::Gesture::TYPE_KEY_TAP:
            {
                Leap::KeyTapGesture tap = gesture;

                InputEvent event;
            
                event.deviceInst = 0;
                event.iValue = g;
                event.fValues[0] = tap.position().x;
                event.fValues[1] = tap.position().y;
                event.fValues[2] = tap.position().z;
                event.fValues[3] = tap.direction().x;
                event.fValues[4] = tap.direction().y;
                event.fValues[5] = tap.direction().z;
                event.deviceType = LeapMotionDeviceType;
                event.objType = SI_KEYTAP_GESTURE;
                event.objInst = 0;
                event.action = SI_GESTURE;
                event.modifier = 0;
            
                Game->postEvent(event);
                break;
            }
            case Leap::Gesture::TYPE_SCREEN_TAP:
            {
                Leap::ScreenTapGesture screentap = gesture;

                InputEvent event;
            
                event.deviceInst = 0;
                event.iValue = g;
                event.fValues[0] = (F32)screentap.state();
                event.fValues[1] = screentap.position().x;
                event.fValues[2] = screentap.position().y;
                event.fValues[3] = screentap.direction().x;
                event.fValues[4] = screentap.direction().y;
                event.deviceType = LeapMotionDeviceType;
                event.objType = SI_SCREENTAP_GESTURE;
                event.objInst = 0;
                event.action = SI_GESTURE;
                event.modifier = 0;
            
                Game->postEvent(event);
                break;
            }
            default:
                Con::warnf("LeapMotionManager::process() - Unknown gesture detected");
                break;
        }
    }

    if (getMouseControlToggle())
    {
        generateMouseEvent(controller);
        return;
    }

    // Is a hand present?
    if ( !frame.hands().empty() ) 
    {
        for (int h = 0; h < frame.hands().count(); ++h)
        {
            const Leap::Hand hand = frame.hands()[h];

            InputEvent handEvent;

            handEvent.deviceInst = 0;
            handEvent.fValues[0] = 0;
            handEvent.deviceType = LeapMotionDeviceType;
            handEvent.objType = LM_HANDPOS;
            handEvent.objInst = h;
            handEvent.action = SI_LEAP;
            handEvent.modifier = 0;

            //Game->postEvent(handEvent);

            const Leap::FingerList fingers = hand.fingers();

            for (int f = 0; f < fingers.count(); ++f)
            {
                InputEvent fingerEvent;

                fingerEvent.deviceInst = 0;
                handEvent.fValues[0] = 0;
                fingerEvent.deviceType = LeapMotionDeviceType;
                fingerEvent.objType = LM_FINGERPOS;
                fingerEvent.objInst = f;
                fingerEvent.action = SI_LEAP;
                fingerEvent.modifier = 0;

                //Game->postEvent(fingerEvent);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void LeapMotionManager::generateMouseEvent(Leap::Controller const & controller)
{

    const Leap::ScreenList screens = controller.calibratedScreens();

    // make sure we have a detected screen
    if (screens.empty())
        return;

    const Leap::Screen screen = screens[0];

    // find the first finger or tool
    const Leap::Frame frame = controller.frame();
    const Leap::HandList hands = frame.hands();

    if (hands.empty())
        return;

    const Leap::PointableList pointables = hands[0].pointables();

    if (pointables.empty())
        return;

    const Leap::Pointable firstPointable = pointables[0];

    // get x, y coordinates on the first screen
    const Leap::Vector intersection = screen.intersect( firstPointable, true, 1.0f );

    // if the user is not pointing at the screen all components of
    // the returned vector will be Not A Number (NaN)
    // isValid() returns true only if all components are finite
    if (!intersection.isValid())
        return;

    unsigned int x = screen.widthPixels() * intersection.x;

    // flip y coordinate to standard top-left origin
    unsigned int y = screen.heightPixels() * (1.0f - intersection.y);

    //Con::printf("Pointable pos: %i %i", x, y);

    Point2I location(x, y);

    // Move the cursor
    Canvas->setCursorPos(Point2I((S32) location.x, (S32) location.y));

    // Build the mouse event
    MouseMoveEvent TorqueEvent;
    TorqueEvent.xPos = (S32) location.x;
    TorqueEvent.yPos = (S32) location.y;

    // Post the event
    Game->postEvent(TorqueEvent);
}

//-----------------------------------------------------------------------------

void LeapMotionManager::MotionListener::onInit(const Leap::Controller& controller)
{
    Con::printf("MotionListener::onInit()");
}

//-----------------------------------------------------------------------------

void LeapMotionManager::MotionListener::onFrame(const Leap::Controller& controller)
{
    gLeapMotionManager->process(controller);
}

//-----------------------------------------------------------------------------

void LeapMotionManager::MotionListener::onConnect (const Leap::Controller &controller)
{
    gLeapMotionManager->setActive(true);
    controller.enableGesture(Leap::Gesture::TYPE_CIRCLE);
    controller.enableGesture(Leap::Gesture::TYPE_KEY_TAP);
    controller.enableGesture(Leap::Gesture::TYPE_SCREEN_TAP);
    controller.enableGesture(Leap::Gesture::TYPE_SWIPE);
}

//-----------------------------------------------------------------------------

void LeapMotionManager::MotionListener::onDisconnect (const Leap::Controller &controller)
{
    gLeapMotionManager->setActive(false);
}