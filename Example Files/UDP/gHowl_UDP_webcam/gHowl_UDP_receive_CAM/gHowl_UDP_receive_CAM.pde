import hypermedia.net.*;
UDP udps; 
import JMyron.*;

int NUM_SQUARES = 40;

JMyron theMov;
int sampleWidth, sampleHeight;
int numSamplePixels;



void setup() {
  size(160, 120);
  udps = new UDP( this, 6000 );
  theMov = new JMyron();
  theMov.start(width, height);
  theMov.findGlobs(0);

  sampleWidth = width/NUM_SQUARES;
  sampleHeight = height/NUM_SQUARES;
  numSamplePixels = sampleWidth*sampleHeight;
}

void draw() {
  theMov.update();
  int[] currFrame = theMov.image();
  String message="";
  // go through all the cells
  for (int y=0; y < height; y += sampleHeight) {
    for (int x=0; x < width; x += sampleWidth) {
      // reset the averages
      float r = 0;
      float g = 0;
      float b = 0;

      // go through all the pixels in the current cell
      for (int yIndex = 0; yIndex < sampleHeight; yIndex++) {
        for (int xIndex = 0; xIndex < sampleWidth; xIndex++) {
          // add each pixel in the current cell's RGB values to the total
          // we have to multiply the y values by the width since we are 
          // using a one-dimensional array
          r += red(currFrame[x+y*width+xIndex+yIndex*width]);
          g += green(currFrame[x+y*width+xIndex+yIndex*width]);
          b += blue(currFrame[x+y*width+xIndex+yIndex*width]); 
        }
      }

      r /= numSamplePixels;
      g /= numSamplePixels;
      b /= numSamplePixels;

      fill(r, g, b);
      rect(x, y, sampleWidth, sampleHeight);
      message = message + (str(r)+","+str(g)+","+str(b)+";");
    }
  }

  
    String ip       = "127.0.0.1";	// the remote IP address
    int port        = 6400;		// the destination port
    // send the message
    udps.send( message, ip, port );
   
    delay(500);
}

public void stop() {
  theMov.stop();
  super.stop();
}

