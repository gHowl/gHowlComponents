/**
 * (./) udp.pde - how to use UDP library as unicast connection
 * (cc) 2006, Cousot stephane for The Atelier Hypermedia
 * (->) http://hypermedia.loeil.org/processing/
 *
 * Create a communication between Processing<->Pure Data @ http://puredata.info/
 * This program also requires to run a small program on Pd to exchange data  
 * (hum!!! for a complete experimentation), you can find the related Pd patch
 * at http://hypermedia.loeil.org/processing/udp.pd
 * 
 * -- note that all Pd input/output messages are completed with the characters 
 * ";\n". Don't refer to this notation for a normal use. --
 */

// import UDP library
import hypermedia.net.*;



/**
 * init
 */
UDP udpr;  // define the UDP object

/**
 * init
 */
void setup() {

  // create a new datagram connection on port 6000
  // and wait for incomming message
  udpr = new UDP( this, 10000);
  //udp.log( true ); 		// <-- printout the connection activity
  udpr.listen( true );
}

//process events
void draw() {
  
redraw();
}
void receive( byte[] data, String ip, int port ) {	// <-- extended handler
  
  
  // get the "real" message =
  // forget the ";\n" at the end <-- !!! only for a communication with Pd !!!
  data = subset(data, 0, data.length);
  String message = new String( data );
  
  // print the result
  println( "receive: \""+message+"\" from "+ip+" on port "+port );
  
  background(255, 0, 255);
  ellipse(width/2, height/2, float(message), float(message));
  
  
}

