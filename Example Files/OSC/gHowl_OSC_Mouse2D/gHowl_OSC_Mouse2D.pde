/*
gHowl Test for OSC sending and receiving
Adapted from the Mouse2D and OSCsendreceive examples 
included with processing and the oscP5 library respectively.
*/

/**
 * Mouse 2D. 
 * 
 * Moving the mouse changes the position and size of each box. 
 */
 import oscP5.*;
import netP5.*;
  
 OscP5 oscP5;
NetAddress myRemoteLocation;
  String message;	   // the message to send
  float x;
  float y;
  boolean rec = false;
void setup() 
{
  size(200, 200); 
   oscP5 = new OscP5(this,12000);//sending from this port

  colorMode(RGB, 255, 255, 255, 100);
  rectMode(CENTER);
  myRemoteLocation = new NetAddress("127.0.0.1",12001);
}

void draw() 
{   
  background(255); 
  fill(255, 50, 255);
  stroke(0);
  if(!rec){
  x = mouseX;
  y = mouseY;
 
  }else{
  x = x;
  y = y;
  rec = false;
  }
     rect(x, y, 20, 20);
  //if mouse position differs from previous [added a check to see if pos differs - Giulio]
  if(mouseX != pmouseX || mouseY != pmouseY){
     OscMessage myMessage = new OscMessage("/gHowlTest");
    message  = String.valueOf(mouseX) + ";" + String.valueOf(mouseY)+" "; // the message to send
    myMessage.add(String.valueOf(mouseX));
    myMessage.add(String.valueOf(mouseY));
    
    // prints the message on the console
    print( message );
    
  /* send the message */
  oscP5.send(myMessage, myRemoteLocation); 
  }
}

void oscEvent(OscMessage theOscMessage) {
  /* print the address pattern and the typetag of the received OscMessage */
  print("### received an osc message.");
  print(" addrpattern: "+theOscMessage.addrPattern());
  println(" typetag: "+theOscMessage.typetag());
  println(" x = "+theOscMessage.get(0).stringValue());
    println(" Y = "+theOscMessage.get(1).stringValue());
    x = float(theOscMessage.get(0).stringValue());
    y = float(theOscMessage.get(1).stringValue());
    rec = true;
 // rect(float(theOscMessage.get(0).stringValue()), float(theOscMessage.get(1).stringValue()), 20, 20);
}
