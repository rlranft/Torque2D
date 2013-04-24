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

function LeapToy::initializeInput( %this )
{
    // General toy action map
    new ActionMap(ToyMap);
    
    // Gesture action map for sandbox
    new ActionMap(GestureMap);
    
    // Absolute pos/rotation action map for game
    new ActionMap(LeapMap);
    
    // Create keyboard bindings
    ToyMap.bindObj(keyboard, tab, toggleCursorMode, %this);
    ToyMap.bindObj(keyboard, escape, showToolBox, %this);

    // Debugging keybinds
    ToyMap.bindObj(keyboard, space, simulateCircle, %this);
    ToyMap.bindObj(keyboard, x, simulateKeytap, %this);
    ToyMap.bindObj(keyboard, z, showParticle, %this);    

    // Create Leap Motion gesture bindings
    GestureMap.bindObj(leapdevice, circleGesture, reactToCircleGesture, %this);
    GestureMap.bindObj(leapdevice, screenTapGesture, reactToScreenTapGesture, %this);
    GestureMap.bindObj(leapdevice, swipeGesture, reactToSwipeGesture, %this);
    GestureMap.bindObj(leapdevice, keyTapGesture, reactToKeyTapGesture, %this);
    
    // Create the Leap Motion hand/finger bindings
    LeapMap.bindObj(leapdevice, leapHandPos, "D", %this.handPosDeadzone, trackHandPosition, %this);
    LeapMap.bindObj(leapdevice, leapHandRot, "D", %this.handRotDeadzone, trackHandRotation, %this);
    LeapMap.bindObj(leapdevice, leapFingerPos, "D", %this.fingerPosDeadzone, trackFingerPos, %this);
    
    // Push the LeapMap to the stack, making it active
    ToyMap.push();
    
    // Initialize the Leap Motion manager
    initLeapMotionManager();
    enableLeapMotionManager(true);
    enableLeapCursorControl(true);
    
    configureLeapGesture("Gesture.Circle.MinProgress", 1);
    configureLeapGesture("Gesture.ScreenTap.MinForwardVelocity", 1);
    configureLeapGesture("Gesture.ScreenTap.MinDistance", 0.1);
}

//-----------------------------------------------------------------------------

function LeapToy::destroyInput(%this)
{
    LeapMap.pop();
    LeapMap.delete();
    
    GestureMap.pop();
    GestureMap.delete();
    
    ToyMap.pop();
    ToyMap.delete();
}

//-----------------------------------------------------------------------------
// Called when the user makes a circle with their finger(s)
//
// %id - Finger ID, based on the order the finger was added to the tracking
// %progress - How much of the circle has been completed
// %radius - Radius of the circle created by the user's motion
// %isClockwise - Toggle based on the direction the user made the circle
// %state - State of the gesture progress: 1: Started, 2: Updated, 3: Stopped
function LeapToy::reactToCircleGesture(%this, %id, %progress, %radius, %isClockwise, %state)
{
    if (!%this.enableCircleGesture)
        return;

    if (%progress > 0 && %state != 2)
    {
        %this.grabObjectsInCircle(%radius/7);        
	    %this.schedule(500, "hideCircleSprite");
    }
    else if (%progress > 0 && %state != 3)
    {
        %this.showCircleSprite(%radius, %isClockwise);
    }
}

//-----------------------------------------------------------------------------
// Called when the user makes a swipe with their finger(s)
//
// %id - Finger ID, based on the order the finger was added to the tracking
// %state - State of the gesture progress: 1: Started, 2: Updated, 3: Stopped
// %direction - 3 point vector based on the direction the finger swiped
// %speed - How fast the user's finger moved. Values will be quite large
function LeapToy::reactToSwipeGesture(%this, %id, %state, %direction, %speed)
{
    if (!%this.enableSwipeGesture)
        return;

    %worldPosition = SandboxWindow.getWorldPoint(Canvas.getCursorPos());

    if (isLeapCursorControlled())
        %worldPosition = "0 0";

    %this.createAsteroid(%worldPosition, %direction, %speed);
}

//-----------------------------------------------------------------------------
// Called when the user makes a screen tap gesture with their finger(s)
//
// %id - Finger ID, based on the order the finger was added to the tracking
// %position - 3 point vector based on where the finger was located in "Leap Space"
// %direction - 3 point vector based on the direction the finger motion
function LeapToy::reactToScreenTapGesture(%this, %id, %position, %direction)
{
    if (!%this.enableScreenTapGesture)
        return;

    %control = Canvas.getMouseControl();
    %control.performClick();
}

//-----------------------------------------------------------------------------
// Called when the user makes a key tap gesture with their finger(s)
//
// %id - Finger ID, based on the order the finger was added to the tracking
// %position - 3 point vector based on where the finger was located in "Leap Space"
// %direction - 3 point vector based on the direction the finger tap
function LeapToy::reactToKeyTapGesture(%this, %id, %position, %direction)
{
    if (!%this.enableKeyTapGesture)
        return;

    %this.deleteSelectedObjects();
}

//-----------------------------------------------------------------------------
// Constantly polling callback based on the palm position of a hand
//
// %id - Ordered hand ID based on when it was added to the tracking device
// %position - 3 point vector based on where the hand is located in "Leap Space"
function LeapToy::trackHandPosition(%this, %id, %position)
{
    echo("Hand " @ %id @ " - x:" SPC %position._0 SPC "y:" SPC %position._1 SPC "z:" SPC %position._2);
}

//-----------------------------------------------------------------------------
// Constantly polling callback based on the palm rotation of a hand
//
// %id - Ordered hand ID based on when it was added to the tracking device
// %rotation - 3 point vector based on the hand's rotation: "yaw pitch roll"
function LeapToy::trackHandRotation(%this, %id, %rotation)
{
    if (isLeapCursorControlled() || !%this.enableHandRotation)
        return;

    // Grab the values. Only going to use pitch and inverse roll
    %yaw = %rotation._0;
    %pitch = %rotation._1;
    %roll = %rotation._2;

    %this.accelerateBall(%roll*-1, %pitch);
}

//-----------------------------------------------------------------------------
// Constantly polling callback based on the finger position on a hand
// %id - Ordered hand ID based on when it was added to the tracking device
// %position - 3 point vector based on where the finger is located in "Leap Space"
function LeapToy::trackFingerPos(%this, %id, %position)
{
    if (!%this.enableFingerTracking)
        return;

    if (!%id)
        echo("Finger " @ %id+1 @ " - x:" SPC %position._0 SPC "y:" SPC %position._1);
}

//-----------------------------------------------------------------------------
// Flips a switch to activate/deactivate cursor control by the Leap Motion Manager
function LeapToy::toggleCursorMode( %this, %val )
{
    if (!%val)
        return;
        
    if (isLeapCursorControlled())
        enableLeapCursorControl(false);
    else
        enableLeapCursorControl(true);
}

//-----------------------------------------------------------------------------
// Shows the Sandbox tool box
function LeapToy::showToolBox( %this, %val )
{
    if (%val)
        toggleToolbox(true);
}

//-----------------------------------------------------------------------------
// DEBUGGING FUNCTIONS
function LeapToy::simulateCircle( %this, %val)
{
    if (%val)
    {
        %this.grabObjectsInCircle(2);
    }
}
function LeapToy::simulateKeyTap( %this, %val )
{
    if (%val)
    {
        %this.deleteSelectedObjects();
    }
}

function LeapToy::showParticle(%this)
{
    %worldPosition = SandboxWindow.getWorldPoint(Canvas.getCursorPos());
    
    %particlePlayer = new ParticlePlayer();
    %particlePlayer.BodyType = static;
    %particlePlayer.SetPosition( %worldPosition );
    %particlePlayer.SceneLayer = 0;
    %particlePlayer.ParticleInterpolation = true;
    %particlePlayer.Particle = "LeapToy:blockFadeParticle";
    %particlePlayer.SizeScale = 1;
    SandboxScene.add( %particlePlayer ); 
}
// DEBUGGING FUNCTIONS
//-----------------------------------------------------------------------------