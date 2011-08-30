 import hypermedia.net.*;
UDP udps;  // define the UDP object

import traer.physics.*;

ParticleSystem physics;
Particle[][] particles;
int gridSize = 10;

float SPRING_STRENGTH = 0.2;
float SPRING_DAMPING = 0.1;

void setup()
{
  size(400, 400);
  smooth();
  fill(0);
        
         udps = new UDP( this, 6005 );
         
  physics = new ParticleSystem(0.1, 0.01);
  
  particles = new Particle[gridSize][gridSize];
  
  float gridStepX = (float) ((width / 2) / gridSize);
  float gridStepY = (float) ((height / 2) / gridSize);
  
  for (int i = 0; i < gridSize; i++)
  {
    for (int j = 0; j < gridSize; j++)
    {
      particles[i][j] = physics.makeParticle(0.2, j * gridStepX + (width / 4), i * gridStepY + 20, 0.0);
      if (j > 0)
      {
        physics.makeSpring(particles[i][j - 1], particles[i][j], SPRING_STRENGTH, SPRING_DAMPING, gridStepX);
      }
    }
  }
  
  for (int j = 0; j < gridSize; j++)
  {
    for (int i = 1; i < gridSize; i++)
    {
      physics.makeSpring(particles[i - 1][j], particles[i][j], SPRING_STRENGTH, SPRING_DAMPING, gridStepY);
    }
  }
  
  particles[0][0].makeFixed();
  particles[0][gridSize - 1].makeFixed();
  
}

void draw()
{
  physics.tick();
  
  if (mousePressed)
  {
    particles[0][gridSize - 1].position().set(mouseX, mouseY, 0);
    particles[0][gridSize - 1].velocity().clear();
  }
  
  noFill();
  
  background(255);
  String message = gridSize+","+gridSize+";";
  for (int i = 0; i < gridSize; i++)
  {
    beginShape();
    curveVertex(particles[i][0].position().x(), particles[i][0].position().y());
    for (int j = 0; j < gridSize; j++)
    {
      curveVertex(particles[i][j].position().x(), particles[i][j].position().y());
      
    }
    curveVertex(particles[i][gridSize - 1].position().x(), particles[i][gridSize - 1].position().y());
    endShape();
  }
  for (int j = 0; j < gridSize; j++)
  {
    beginShape();
    curveVertex(particles[0][j].position().x(), particles[0][j].position().y());
    for (int i = 0; i < gridSize; i++)
    {
      curveVertex(particles[i][j].position().x(), particles[i][j].position().y());
      
      message = message + (String.valueOf(particles[i][j].position().x())+","+particles[i][j].position().y()+";");
      //print(message);
    }
    curveVertex(particles[gridSize - 1][j].position().x(), particles[gridSize - 1][j].position().y());
    endShape();
    
  }
  //send message
    String ip       = "127.0.0.1";	// the remote IP address
    int port        = 6406;		// the destination port
    udps.send( message, ip, port ); 
}

void mouseReleased()
{
  particles[0][gridSize - 1].velocity().set( (mouseX - pmouseX), (mouseY - pmouseY), 0 );
}
