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
    // Create a new ActionMap
    new ActionMap(LeapMap);
    
    // Create keyboard bindings
    LeapMap.bindObj(keyboard, space, pickSprite, %this);
    LeapMap.bindObj(keyboard, z, toggleCursorMode, %this);
    
    // Create Leap Motion bindings
    //LeapMap.bindObj(leapdevice, circleGesture, reactToCircleGesture, %this);
    //LeapMap.bindObj(leapdevice, swipeGesture, reactToSwipeGesture, %this);
    //LeapMap.bindObj(leapdevice, screenTapGesture, reactToScreenTapGesture, %this);
    //LeapMap.bindObj(leapdevice, keyTapGesture, reactToKeyTapGesture, %this);
    //LeapMap.bindObj(leapdevice, leapHandPos, "D", %this.handPosDeadzone, trackHandPosition, %this);
    //LeapMap.bindObj(leapdevice, leapHandRot, "D", %this.handRotDeadzone, trackHandRotation, %this);
    //LeapMap.bindObj(leapdevice, leapFingerPos, "D", %this.fingerPosDeadzone, trackFingerPos, %this);

    // Push the LeapMap to the stack, making it active
    LeapMap.push();
    
    // Initialize the Leap Motion manager
    initLeapMotionManager();
    enableLeapMotionManager(true);
    enableLeapCursorControl(true);
    
    // Make this toy a listener for input (used purely for onTouchMoved in this case)
    SandboxWindow.addInputListener( %this );
}

//-----------------------------------------------------------------------------
// Callback for when a mouse is moving in SceneWindow. More or less desktop only.
// in this toy, it will move a joint around based on the cursor movement. Any
// attached objects should move with it
//
// %touchID - Ordered device ID based on when it was tracked
// %worldPosition - Where in the Scene the current cursor is located
function LeapToy::onTouchMoved(%this, %touchID, %worldPosition)
{
    // Finish if nothing is being pulled.
    if ( !isObject(Sandbox.ManipulationPullObject[%touchID]) )
        return;
        
    // Set a new target for the target joint.
    SandboxScene.setTargetJointTarget( Sandbox.ManipulationPullJointId[%touchID], %worldPosition );
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
    if (%progress > 0 && %state != 3)
        %this.showCircleSprite(%radius / 2, %isClockwise);
    else
        %this.hideCircleSprite();
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
    //echo("Swipe Gesture - directionX: " @ %direction._0 @ " directionY: " @ %direction._1 @ " directionZ: " @ %direction._2 @ " speed: " @ %speed);
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
    echo("Screen Tap Gesture - positionX: " @ %position._0 @ " positionY: " @ %position._1 @ " directionX: " @ %direction._0 @ " directionY: " @ %direction._1);
}

//-----------------------------------------------------------------------------
// Called when the user makes a key tap gesture with their finger(s)
//
// %id - Finger ID, based on the order the finger was added to the tracking
// %position - 3 point vector based on where the finger was located in "Leap Space"
// %direction - 3 point vector based on the direction the finger tap
function LeapToy::reactToKeyTapGesture(%this, %id, %position, %direction)
{
    echo("Key Tap Gesture - positionX: " @ %position._0 @ " positionY: " @ %position._1 @ " directionX: " @ %direction._0 @ " directionY: " @ %direction._1);
}

//-----------------------------------------------------------------------------
// Constantly polling callback based on the palm position of a hand
//
// %id - Ordered hand ID based on when it was added to the tracking device
// %position - 3 point vector based on where the hand is located in "Leap Space"
function LeapToy::trackHandPosition(%this, %id, %position)
{
    //echo("Hand " @ %id @ " - x:" SPC %position._0 SPC "y:" SPC %position._1 SPC "z:" SPC %position._2);

    %xPosition = %position._0;
    %yPosition = %position._1;
    %zPosition = %position._2;

    %this.accelerateBall(%xPosition, %yPosition, 0);
}

//-----------------------------------------------------------------------------
// Constantly polling callback based on the palm rotation of a hand
//
// %id - Ordered hand ID based on when it was added to the tracking device
// %rotation - 3 point vector based on the hand's rotation: "yaw pitch roll"
function LeapToy::trackHandRotation(%this, %id, %rotation)
{
    //echo("Hand " @ %id @ " - yaw:" SPC %rotation._0 SPC "pitch:" SPC %rotation._1 SPC "roll:" SPC %rotation._2);
    %yaw = %rotation._0;
    %pitch = %rotation._1;
    %roll = %rotation._2;

    %this.accelerateBall(%yaw, %pitch, %roll);
}

//-----------------------------------------------------------------------------
// Constantly polling callback based on the finger position on a hand
// %id - Ordered hand ID based on when it was added to the tracking device
// %position - 3 point vector based on where the finger is located in "Leap Space"
function LeapToy::trackFingerPos(%this, %id, %position)
{
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