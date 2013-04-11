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
    // Set the sandbox drag mode availability.
    Sandbox.allowManipulation( pull );
    
    // Set the manipulation mode.
    Sandbox.useManipulation( pull );
    
    // Width of the ground objects collide with
    LeapToy.GroundWidth = 40;
    
    new ActionMap(LeapMap);
    
    LeapMap.bindObj(keyboard, space, pickSprite, %this);
    LeapMap.bindObj(leapdevice, circleGesture, reactToCircleGesture, %this);
    LeapMap.bindObj(leapdevice, swipeGesture, reactToSwipeGesture, %this);
    LeapMap.bindObj(leapdevice, screenTapGesture, reactToScreenTapGesture, %this);
    LeapMap.bindObj(leapdevice, keyTapGesture, reactToKeyTapGesture, %this);
    
    LeapMap.push();
    
    SandboxWindow.addInputListener( %this );
    
    // Reset the toy.
    LeapToy.reset();
}

//-----------------------------------------------------------------------------

function LeapToy::destroy( %this )
{
    LeapMap.pop();
    LeapMap.delete();
    
    SandboxWindow.removeInputListener( %this );  
}

//-----------------------------------------------------------------------------

function LeapToy::onTouchMoved(%this, %touchID, %worldPosition)
{
    // Finish if nothing is being pulled.
    if ( !isObject(Sandbox.ManipulationPullObject[%touchID]) )
        return;
        
    // Set a new target for the target joint.
    SandboxScene.setTargetJointTarget( Sandbox.ManipulationPullJointId[%touchID], %worldPosition );
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
}

//-----------------------------------------------------------------------------

function LeapToy::createBackground( %this )
{    
    // Create the sprite.
    %object = new Sprite();
    
    // Set the sprite as "static" so it is not affected by gravity.
    %object.setBodyType( static );
       
    // Always try to configure a scene-object prior to adding it to a scene for best performance.

    // Set the position.
    %object.Position = "0 0";

    // Set the size.        
    %object.Size = "40 30";
    
    // Set to the furthest background layer.
    %object.SceneLayer = 31;
    
    // Set an image.
    %object.Image = "ToyAssets:jungleSky";
            
    // Add the sprite to the scene.
    SandboxScene.add( %object );   
}

//-----------------------------------------------------------------------------

function LeapToy::createGround( %this )
{
    // Create the ground
    %ground = new Scroller();
    %ground.setBodyType("static");
    %ground.Image = "ToyAssets:dirtGround";
    %ground.setPosition(0, -12);
    %ground.setSize(LeapToy.GroundWidth, 6);
    %ground.setRepeatX(LeapToy.GroundWidth / 60);   
    %ground.createEdgeCollisionShape(LeapToy.GroundWidth/-2, 3, LeapToy.GroundWidth/2, 3);
    SandboxScene.add(%ground);  
    
    // Create the grass.
    %grass = new Sprite();
    %grass.setBodyType("static");
    %grass.Image = "ToyAssets:grassForeground";
    %grass.setPosition(0, -8.5);
    %grass.setSize(LeapToy.GroundWidth, 2); 
    SandboxScene.add(%grass);       
}

//-----------------------------------------------------------------------------

function LeapToy::createBall( %this )
{
    // Create the ball.
    %ball = new Sprite();
    %ball.Position = "5 5";
    %ball.Size = 2;
    %ball.Image = "ToyAssets:Football";        
    %ball.setDefaultDensity( 0.1 );
    %ball.setDefaultRestitution( 0.5 );
    %ball.createCircleCollisionShape( 1 );
    
    // Add to the scene.
    SandboxScene.add( %ball );
}

//-----------------------------------------------------------------------------

function LeapToy::pickSprite(%this, %val)
{
    %cursorPosition = Canvas.getCursorPos();
    %worldPosition = SandboxWindow.getWorldPoint(%cursorPosition);
        
    if (%val)
    {
        Sandbox.InputController.onTouchDown(0, %worldPosition);
    }
    else
    {
        Sandbox.InputController.onTouchUp(0, %worldPosition);
    }
}

//-----------------------------------------------------------------------------

function LeapToy::reactToCircleGesture(%this, %state, %radius, %angle, %isClockwise)
{
    echo("Circle Gesture - state: " @ %state @ " radius: " @ %radius @ " angle: " @ %angle @ " isClockwise: " @ %isClockwise);
}

//-----------------------------------------------------------------------------

function LeapToy::reactToSwipeGesture(%this, %directionX, %directionY, %directionZ, %speed)
{
    echo("Swipe Gesture - directionX: " @ %directionX @ " directionY: " @ %directionY @ " directionZ: " @ %directionZ @ " speed: " @ %speed);
}

//-----------------------------------------------------------------------------

function LeapToy::reactToScreenTapGesture(%this, %positionX, %positionY, %directionX, %directionY)
{
    echo("Screen Tap Gesture - positionX: " @ %positionX @ " positionY: " @ %positionY @ " directionX: " @ %directionX @ " directionY: " @ %directionY);
}

//-----------------------------------------------------------------------------

function LeapToy::reactToKeyTapGesture(%this, %positionX, %positionY, %diretionX, %directionY)
{
    echo("Key Tap Gesture - positionX: " @ %positionX @ " positionY: " @ %positionY @ " directionX: " @ %directionX @ " directionY: " @ %directionY);
}