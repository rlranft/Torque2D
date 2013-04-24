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

function LeapToy::createBreakoutLevel( %this )
{
    LeapMap.push();
    
    // Create background.
    %this.createBackground();
    
    // Create the ground.
    %this.createGround();
    
    // Create the breakable bricks
    %this.createBricks();
    
    %this.createBall();
    %this.ball.setPosition(-5, -5);
    %dealsDamage = %this.DealsDamageBehavior.createInstance();
    %dealsDamage.initialize(10, false, "");
    
    %this.ball.addBehavior(%dealsDamage);
}

//-----------------------------------------------------------------------------

function LeapToy::createBricks( %this )
{
    // Fetch the block count.
    %brickCount = LeapToy.BrickRows;
    %brickColumsn = LeapToy.BrickColumns;

    // Sanity!
    if ( %brickCount < 1 )
    {
        echo( "Cannot have a brick count count less than one." );
        return;
    }

    // Set the block size.
    %brickSize = LeapToy.BrickSize;

    // Calculate a block building position.
    %posx = %brickCount * %brickSize._0 * -1;
    %posy = 3 + (%brickSize._1 * 0.5) + 3;

    // Build the stack of blocks.
    for( %stack = 0; %stack < %brickCount; %stack++ )
    {
        // Calculate the stack position.
        %stackIndexCount = %brickCount;
        
        %stackX = %posX + ( %stack * %brickSize._0 );
        %stackY = %posY + ( %stack * %brickSize._1 );

        // Build the stack.
        for ( %stackIndex = 0; %stackIndex < LeapToy.BrickColumns; %stackIndex++ )
        {
            // Calculate the block position.
            %brickX = (%stackIndex*%brickSize._0)+%posx;
            %brickY = %stackY;
            %brickFrames = "0 2 4 6 8 10";
            %randomNumber = getRandom(0, 6);
            
            // Create the sprite.
            %obj = new Sprite()
            {
                class = "Brick";
                flippd = false;
            };            
            
            %obj.setPosition( %brickX, %brickY );
            %obj.setSize( %brickSize );
            %obj.setBodyType("Kinematic");
            %obj.setImage( "LeapToy:objectsBricks" );            
            %obj.setImageFrame( getWord(%brickFrames, %randomNumber) );
            %obj.setDefaultFriction( 1.0 );
            %obj.createPolygonBoxCollisionShape( %brickSize );
            %obj.CollisionCallback = true;
            
            %takesDamage = %this.TakesDamageBehavior.createInstance();
            %takesDamage.initialize(20, 100, 10, 0, "", "LeapToy:blockFadeParticle");
            
            %obj.addBehavior(%takesDamage);
            
            // Add to the scene.
            SandboxScene.add( %obj );
        }
    }
}

//-----------------------------------------------------------------------------

function Brick::onCollision(%this, %object, %collisionDetails)
{
    
}