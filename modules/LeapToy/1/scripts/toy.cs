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

    %object.createEdgeCollisionShape( -20, -15, -20, 15 );
    %object.createEdgeCollisionShape( 20, -15, 20, 15 );
    %object.createEdgeCollisionShape( -20, 15, 20, 15 );

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
    %circle.setBodyType("static");
    %circle.Size = 1;
    %circle.Image = "ToyAssets:Crosshair2";
    %circle.Visible = false;
    %this.circleSprite = %circle;

    // Add to the scene.
    SandboxScene.add( %circle );
}

//-----------------------------------------------------------------------------
// This will be called when the user presses the spacebar
//
// %val - Will be true if the spacebar is down, false if it was released
function LeapToy::pickSprite( %this, %val )
{
    // Find out where the cursor is located, then convert to world coordinates
    %cursorPosition = Canvas.getCursorPos();
    %worldPosition = SandboxWindow.getWorldPoint(%cursorPosition);
    
    // If true, force an onTouchDown for the Sandbox input listener
    // If false, force an onTouchUp for the Sandbox input listener    
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
// This will be callsed when the user makes a circle gesture with a Leap Motion
//
// %radius - How large the circle gesture was
// %isClockwise - True if the motion was clockwise
function LeapToy::showCircleSprite( %this, %radius, %isClockwise )
{
    // If it isn't currently visible, show it
    if (!%this.circleSprite.visible)
    {
        %this.circleSprite.visible = true;
        
        // Find out where the cursor is currenty location
        %worldPosition = SandboxWindow.getWorldPoint(Canvas.getCursorPos());
        
        // Move the circle to that spot. This should be where the circle
        // gesture first started
        %this.circleSprite.position = %worldPosition;
    }
    
    // Resize the circle based on how big the radius was
    %this.circleSprite.size = %radius;
    
    // Rotate to the right if the circle is clockwise, left otherwise
    if (%isClockwise)
        %this.circleSprite.AngularVelocity = -180;
    else
        %this.circleSprite.AngularVelocity = 180;
}

//-----------------------------------------------------------------------------
// This will be called when the user stops making a circle gesture
function LeapToy::hideCircleSprite( %this )
{
    // Hide the sprite
    %this.circleSprite.visible = 0;
}

//-----------------------------------------------------------------------------
// This is called when a user makes a swipe gesture with the Leap Motion
//
// %position - Where to spawn the asteroid
// %direction - 3 point vector based on the direction the finger swiped
// %speed - How fast the user's finger moved. Values will be quite large
function LeapToy::createAsteroid( %this, %position, %direction, %speed )
{
    // Size of the asteroid.
    %size = 3;
    
    // Reduce the speed of the swipe so it can be used for a reasonable
    // velocity in T2D.
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

    // Assign the trail to the asteroid, used for cleanup later
    %object.Trail = %player;
}

//-----------------------------------------------------------------------------
// Called when an object with a class of "Asteroid" collides with another body.
// In this toy, it will delete the asteroid and create an explosion
//
// %object - What the asteroid collided with
// %collisionDetails - Information about the collision
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