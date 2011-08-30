// Flocking
// Daniel Shiffman <http://www.shiffman.net>
// The Nature of Code, Spring 2009

// Flock class
// Does very little, simply manages the ArrayList of all the boids




class Flock {
  
  ArrayList boids; // An arraylist for all the boids
  
  

  Flock() {
    boids = new ArrayList(); // Initialize the arraylist
    
  }

  void run() {
    
    String locList = "";
    for (int i = 0; i < boids.size(); i++) {
      Boid b = (Boid) boids.get(i);  
      b.run(boids);  // Passing the entire list of boids to each boid individually
      //println(b.loc.x + " " + b.loc.y);
    //locList.add(b.loc.x + " " + b.loc.y
    locList = locList + String.valueOf(b.loc.x) + "," + String.valueOf(b.loc.y)+";";
      
      
    }
      //send
     String message  = locList;//str( "hello" );	// the message to send
    String ip       = "127.0.0.1";	// the remote IP address
    int port        = 6400;		// the destination port
    //print( message );
    // formats the message for Pd
    //message = message;//+";\n";
    // send the message
    udps.send( message, ip, port );
    locList = "";
  }

  void addBoid(Boid b) {
    boids.add(b);
  }

}
