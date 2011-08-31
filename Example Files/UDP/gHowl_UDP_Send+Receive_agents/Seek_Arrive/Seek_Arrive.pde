// Seek_Arrive
// Daniel Shiffman <http://www.shiffman.net>
// Nature of Code, Spring 2009

// Two "boids" follow the mouse position

// Implements Craig Reynold's autonomous steering behaviors
// One boid "seeks"
// One boid "arrives"
// See: http://www.red3d.com/cwr/

Boid seeker;
Boid arriver;

// import UDP library
import hypermedia.net.*;
UDP udpr;  // define the UDP object;
UDP udps;  // define the UDP object;
String message;
int mx=0;
int my=0;

void setup() {
  size(200,200);
  seeker = new Boid(new PVector(width/2+50,height/2),4.0,0.1);
  arriver = new Boid(new PVector(width/2-50,height/2),4.0,0.1);
  
   // create a new datagram connection on port 6000
  // and wait for incomming message
  udpr = new UDP( this, 7000);
  udps = new UDP( this, 6999);
  //udp.log( true ); 		// <-- printout the connection activity
  udpr.listen( true );
  
  smooth();
}

void draw() {
  background(255);
  
  
  String[] coo = split(message, ',');
  //print(coo.length);
if (coo != null)
{
mx = int(coo[0]);
my = int(coo[1]);
}


  fill(175);
  stroke(0);
  ellipse(mx,my,30,30);
  
  // Call the appropriate steering behaviors for our agents
  seeker.seek(new PVector(mx,my));
  seeker.run();
  arriver.arrive(new PVector(mx,my));
  arriver.run(); 
  
  
  String message2  = seeker.loc.x + "," + seeker.loc.y + ";"+ arriver.loc.x + "," + arriver.loc.y;//str( "hello" );	// the message to send
    String ip       = "127.0.0.1";	// the remote IP address
    int port        = 7001;		// the destination port
    print(message2);
    udps.send( message2, ip, port );
    
}

void receive( byte[] data, String ip, int port ) {	// <-- extended handler
  
  
  // get the "real" message =
  // forget the ";\n" at the end <-- !!! only for a communication with Pd !!!
  data = subset(data, 0, data.length);
  message = new String( data );
  
  // print the result
  println( "receive: \""+message+"\" from "+ip+" on port "+port );
  
  
  
}

