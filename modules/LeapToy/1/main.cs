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

function LeapToy::create( %this )
{
    // Execute toy scripts
    exec("./scripts/input.cs");
    exec("./scripts/toy.cs");

    %this.handPosDeadzone = "-1.0 1.0";
    %this.handRotDeadzone = "-1.0 1.0";
    %this.fingerPosDeadzone = "-1.0 1.0";

    // Set the sandbox drag mode availability.
    Sandbox.allowManipulation( pull );
    
    // Set the manipulation mode.
    Sandbox.useManipulation( pull );
    
    // Width of the ground objects collide with
    LeapToy.GroundWidth = 40;

    %this.initializeInput();
    
    // Reset the toy.
    LeapToy.reset();
}

//-----------------------------------------------------------------------------

function LeapToy::destroy( %this )
{
    // Turn on Leap driven cursor control, if it was activated
    if (isLeapCursorControlled())
        enableLeapMotionManager(false);
        
    // Clean up the Leap ActionMap
    LeapMap.pop();
    LeapMap.delete();
    
    // Tell this toy to stop listening for input events
    SandboxWindow.removeInputListener( %this );  
}

//-----------------------------------------------------------------------------

function LeapToy::reset( %this )
{
    // Clear the scene.
    SandboxScene.clear();
    
    // Set the camera size.
    SandboxWindow.setCameraSize( 40, 30 );
       
    // Create background.
    %this.createBackground();
    
    // Create the ground.
    %this.createGround();
    
    // Create a ball.
    %this.createBall();

    // Create circle gesture visual.
    %this.createCircleSprite();
}