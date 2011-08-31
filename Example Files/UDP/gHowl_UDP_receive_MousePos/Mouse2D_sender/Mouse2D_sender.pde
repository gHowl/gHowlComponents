/**
 * Mouse 2D. 
 * 
 * Moving the mouse changes the position and size of each box. 
 */
  import hypermedia.net.*;
  UDP udps;                // define the UDP object
  
  String message;	   // the message to send
  String ip = "127.0.0.1"; // the remote IP address
  int port = 9000;	   // the remote port
 
void setup() 
{
  size(200, 200); 
  udps = new UDP( this, 6005 ); //sending from this port

  colorMode(RGB, 255, 255, 255, 100);
  rectMode(CENTER);
}

void draw() 
{   
  background(255); 
  fill(255, 50, 255);
  stroke(0);
  rect(mouseX, mouseY, 20, 20);
    
  //if mouse position differs from previous [added a check to see if pos differs - Giulio]
  if(mouseX != pmouseX || mouseY != pmouseY){
    
    message  = String.valueOf(mouseX) + ";" + String.valueOf(mouseY)+" "; // the message to send
    
    // prints the message on the console
    print( message );
    
    // sends the message
    udps.send( message, ip, port );
  }
}
