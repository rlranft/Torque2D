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
    //LeapMap.bindObj(leapdevice, screenTapGesture, reactToScreenTapGesture, %this);
    //LeapMap.bindObj(leapdevice, keyTapGesture, reactToKeyTapGesture, %this);
    //LeapMap.bindObj(leapdevice, leapHandPos, trackHandPosition, %this);
    //LeapMap.bindObj(leapdevice, leapHandRot, trackHandRotation, %this);
    LeapMap.push();
    
    SandboxWindow.addInputListener( %this );

    initLeapMotionManager();
    enableLeapMotionManager(true);
    enableLeapCursorControl(true);

    // Reset the toy.
    LeapToy.reset();
}

//-----------------------------------------------------------------------------

function LeapToy::destroy( %this )
{
    enableLeapMotionManager(false);
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

    // Create circle gesture visual.
    %this.createCircleSprite();
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
    %ground.SceneLayer = 11;
    %ground.setSize(LeapToy.GroundWidth, 6);
    %ground.setRepeatX(LeapToy.GroundWidth / 60);   
    %ground.createEdgeCollisionShape(LeapToy.GroundWidth/-2, 3, LeapToy.GroundWidth/2, 3);
    SandboxScene.add(%ground);  
    
    // Create the grass.
    %grass = new Sprite();
    %grass.setBodyType("static");
    %grass.Image = "ToyAssets:grassForeground";
    %grass.setPosition(0, -8.5);
    %grass.SceneLayer = 12;
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

function LeapToy::createCircleSprite( %this )
{
    // Create the circle.
    %circle = new Sprite();
    %circle.Position = "0 0";
    %circle.Size = 1;
    %circle.Image = "ToyAssets:Crosshair2";
    %circle.Visible = false;
    %this.circleSprite = %circle;

    // Add to the scene.
    SandboxScene.add( %circle );
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

function LeapToy::showCircleSprite( %this, %radius, %isClockwise )
{
    if (!%this.circleSprite.visible)
    {
        %worldPosition = SandboxWindow.getWorldPoint(Canvas.getCursorPos());
        %this.circleSprite.position = %worldPosition;
        %this.circleSprite.visible = true;
    }
    
    %this.circleSprite.size = %radius;
    
    if (%isClockwise)
        %this.circleSprite.AngularVelocity = -180;
    else
        %this.circleSprite.AngularVelocity = 180;
}

//-----------------------------------------------------------------------------

function LeapToy::sizeCircleSprite( %this, %radius, %isClockwise )
{
    %this.circleSprite.size = %radius;
    
    if (%isClockwise)
        %this.circleSprite.AngularVelocity = -180;
    else
        %this.circleSprite.AngularVelocity = 180;
}

//-----------------------------------------------------------------------------

function LeapToy::hideCircleSprite( %this )
{
    %this.circleSprite.visible = 0;
}

//-----------------------------------------------------------------------------

function LeapToy::createAsteroid( %this, %position, %direction, %speed )
{
    %size = 3;
    %reducedSpeed = mClamp((%speed / 10), 0, 55);

    %velocity = vectorScale(%direction, %reducedSpeed);

    // Create an asteroid.
    %object = new Sprite()
    {
        class = "Asteroid";
    };

    %object.Position = %position;
    %object.CollisionCallback = true;
    %object.Size = %size;
    %object.SceneLayer = 8;
    %object.Image = "ToyAssets:Asteroids";
    %object.ImageFrame = getRandom(0,3);
    %object.setDefaultDensity( 0.2 );
    %object.createCircleCollisionShape( %size * 0.4 );
    %object.setLinearVelocity( %velocity._0, %velocity._1 );
    %object.setAngularVelocity( getRandom(-90,90) );
    %object.setLifetime( 10 );
    SandboxScene.add( %object );

    // Create fire trail.
    %player = new ParticlePlayer();
    %player.Particle = "ToyAssets:bonfire";
    %player.Position = %object.Position;
    %player.EmissionRateScale = 3;
    %player.SizeScale = 2;
    %player.SceneLayer = 0;
    %player.setLifetime( 10 );
    SandboxScene.add( %player );
    %jointId = SandboxScene.createRevoluteJoint( %object, %player );
    SandboxScene.setRevoluteJointLimit( %jointId, 0, 0 );

    %object.Trail = %player;

    return %object;
}

//-----------------------------------------------------------------------------

function Asteroid::onCollision( %this, %object, %collisionDetails )
{
    // Create explosion.
    %player = new ParticlePlayer();
    %player.BodyType = static;
    %player.Particle = "ToyAssets:impactExplosion";
    %player.Position = %this.Position;
    %player.SceneLayer = 0;
    SandboxScene.add( %player );

    // Delete the asteroid.
    %this.Trail.LinearVelocity = 0;
    %this.Trail.AngularVelocity = 0;
    %this.Trail.safeDelete();
    %this.safeDelete();
}

//-----------------------------------------------------------------------------

function LeapToy::reactToCircleGesture(%this, %id, %progress, %radius, %isClockwise, %state)
{
    if (%progress > 0 && %state != 3)
        %this.showCircleSprite(%radius / 2, %isClockwise);
    else
        %this.hideCircleSprite();
}

//-----------------------------------------------------------------------------

function LeapToy::reactToSwipeGesture(%this, %id, %state, %direction, %speed)
{
    //echo("Swipe Gesture - directionX: " @ %direction._0 @ " directionY: " @ %direction._1 @ " directionZ: " @ %direction._2 @ " speed: " @ %speed);
    %worldPosition = SandboxWindow.getWorldPoint(Canvas.getCursorPos());

    if (isLeapCursorControlled())
        %worldPosition = "0 0";

    %this.createAsteroid(%worldPosition, %direction, %speed);
}

//-----------------------------------------------------------------------------

function LeapToy::reactToScreenTapGesture(%this, %id, %position, %direction)
{
    echo("Screen Tap Gesture - positionX: " @ %position._0 @ " positionY: " @ %position._1 @ " directionX: " @ %direction._0 @ " directionY: " @ %direction._1);
}

//-----------------------------------------------------------------------------

function LeapToy::reactToKeyTapGesture(%this, %id, %position, %direction)
{
    echo("Key Tap Gesture - positionX: " @ %position._0 @ " positionY: " @ %position._1 @ " directionX: " @ %direction._0 @ " directionY: " @ %direction._1);
}

//-----------------------------------------------------------------------------

function LeapToy::trackHandPosition(%this, %id, %position)
{
    echo("Hand " @ %id @ " - x:" SPC %position._0 SPC "y:" SPC %position._1 SPC "z:" SPC %position._2);
}

//-----------------------------------------------------------------------------

function LeapToy::trackHandRotation(%this, %id, %rotation)
{
    echo("Hand " @ %id @ " - yaw:" SPC %rotation._0 SPC "pitch:" SPC %rotation._1 SPC "roll:" SPC %rotation._2);
}