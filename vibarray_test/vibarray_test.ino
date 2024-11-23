#include <Arduino.h>
#include <Wire.h>
#include "Adafruit_ZeroTimer.h"
#include "SAMD_PWM.h"
// Aruduino pins for data output to throughhand2.
// See the datasheet of NPIC6C4894 (12-bit shift register) to understand these pin names.
#define LATCH 11
#define CLOCK 12
#define DATA 13

#define SIG 7


// picture frame: X by Y picture
#define X 12  // horizontal (to left): x = 0 .. X - 1
#define Y 18  // vertical (downward): y = 0 .. Y - 1
int frame[X][Y];
int frame_info[X][Y];
// ThroughHand2 data frame: 18 * 12 bits
#define I 18  // i = 0 .. I - 1
#define J 12  // j = 0 .. J - 1


//PWM duty cycle
#define _PWM_LOGLEVEL_       1
#define DIR 9
#define PWM_PIN 10

SAMD_PWM* PWM_Instance;
float dutyCycle= 0.0f;
float pwm_freq= 20000.0f;
uint8_t channel = 0;
int intensity = 2;

typedef struct
{
  uint16_t level;
} PWD_Data;

// Data for 0-100%
PWD_Data PWM_data[] =
{
  {    0 },
  { ( 20 * 65535 ) / 100 },
  { ( 40 * 65535 ) / 100 },
  { ( 60 * 65535 ) / 100 },
  { ( 80 * 65535 ) / 100 },
  { (100 * 65535 ) / 100 },
  { ( 80 * 65535 ) / 100 },
  { ( 60 * 65535 ) / 100 },
  { ( 40 * 65535 ) / 100 },
  { ( 20 * 65535 ) / 100 },
};
#define NUM_PWM_POINTS      ( sizeof(PWM_data) / sizeof(PWD_Data) )
PWD_Data PWM_data_idle = PWM_data[0];

//Timer Interrupt
float timer_freq = 5000.0f;
Adafruit_ZeroTimer zerotimer = Adafruit_ZeroTimer(5);

void TC5_Handler() {
  Adafruit_ZeroTimer::timerHandler(5);
}

//Distance Sensor
#define dir_1 16
#define dis_1 15
#define dir_2 17
#define dis_2 18
volatile int encode_1 = 0; //num of pulse
volatile int encode_2 = 0; //num of pulse
volatile int old_1 = 0;
volatile int old_2 = 0;

void clock_low(){
  PORT->Group[g_APinDescription[CLOCK].ulPort].OUTCLR.reg = (1 << g_APinDescription[CLOCK].ulPin);
}

void data_low(){
  PORT->Group[g_APinDescription[DATA].ulPort].OUTCLR.reg = (1 << g_APinDescription[DATA].ulPin);
}

void latch_low(){
  PORT->Group[g_APinDescription[LATCH].ulPort].OUTCLR.reg = (1 << g_APinDescription[LATCH].ulPin);
}

void clock_high(){
  PORT->Group[g_APinDescription[CLOCK].ulPort].OUTSET.reg = (1 << g_APinDescription[CLOCK].ulPin);
}

void data_high(){
  PORT->Group[g_APinDescription[DATA].ulPort].OUTSET.reg = (1 << g_APinDescription[DATA].ulPin);
}

void latch_high(){
  PORT->Group[g_APinDescription[LATCH].ulPort].OUTSET.reg = (1 << g_APinDescription[LATCH].ulPin);
}

// reset the frame.
void zeroFrame(){
  for(int y = 0; y < Y; y++){
    for(int x = 0; x < X; x++){
      frame[x][y] = 0;
    }
  }
}

void oneFrame(){
  for(int y = 0; y < Y; y++){
    for(int x = 0; x < X; x++){
      frame[x][y] = 1;
    }
  }
}

// set the data line.
inline void setData(int v){
  if(v > 0) data_high();
  else data_low();
}

// output one cycle to the clock line.
inline void push(){
  delayMicroseconds(1);
  clock_high();
  delayMicroseconds(1);
  clock_low();
}

// move the internal shift register data to the output ports.
inline void latch(){
  latch_high();
  delayMicroseconds(1);
  latch_low();
}

#define I2 (I/2)
#define J2 (J/2)
#define X2 (X/2)
#define Y2 (Y/2)

void sendFrame(){
  int i, j, x, y;
  for(i = 0; i < I2; i++){
    y = Y - 1 - 2 * i;
    for(j = 0; j < J2; j++){
      x = X2 + j;
      setData(frame[x][y]);
      push();
    }
    y--;
    for(j = J2; j < J; j++){
      x = j;
      setData(frame[x][y]);
      push();
    }
  }
  for(i = I2; i < I; i++){
    y = 2 * i - Y;
    for(j = 0; j < J2; j++){
      x = X2 - 1 - j;
      setData(frame[x][y]);
      push();
    }
    y++;
    for(j = J2; j < J; j++){
      x = X - 1 - j;
      setData(frame[x][y]);
      push();
    }
  }
  //latch();
}

void bytepattern(byte* buf_1){
  for(int i= 0; i< 108; i ++){
    int y= i / 6;
    int x = (2* i) % 12;
    byte b = (byte)buf_1[i];
    int first = int(b/6);
    int second = int(b%6);
    if(first != 0){
      intensity = first;
      frame[x][y] = 1;
      }
    else{
      frame[x][y] = 0;
    }

    if(second != 0){
      intensity = second;
      frame[x+1][y] =1;
      }
    else{
      frame[x+1][y] = 0;
    }

    //frame[x][y] = int(b / 6);
    //frame[x+1][y] =int(b % 6);
  }
}

void pattern_now(int num){
  for(int x=0; x<X; x++){
    for(int y=0; y<Y; y++){
      if(frame_info[x][y] >num){
        frame[x][y] = 1;
      }
      else{
        frame[x][y] = 0;
      }
    }
  }
  //printFrame();
}

volatile int inter = 0;

int sin_wave[10] = {305, 590, 819, 972, 1024, 1024, 972, 819, 590, 305};
float intense[6] = {0, 0.2, 0.4, 0.65, 0.85, 1.0};
void TimerHandler(void){
  if(inter ==0){
    digitalWrite(DIR, LOW);
  }
  if(inter ==10){
    digitalWrite(DIR, HIGH);
    }
  PWM_Instance->setPWM_manual(PWM_PIN, (PWM_data[(inter%10)].level)* intense[intensity]);
  inter ++;
  if(inter >= 20){inter = 0;}

}

void handler_1(){
  int direction = digitalRead(dir_1);
  int dist = digitalRead(dis_1);
  /*// Right: 
  if(direction ==HIGH && dist == HIGH){encode_1++;}
  if(direction ==LOW && dist == HIGH){encode_1--;}*/
  //Left
  if(direction ==HIGH && dist == HIGH){encode_1--;}
  if(direction ==LOW && dist == HIGH){encode_1++;}
  }

void handler_2(){
  int direction = digitalRead(dir_2);
  int dist = digitalRead(dis_2);
  /* //Right:
  if(direction ==HIGH && dist == HIGH){encode_2--;}
  if(direction ==LOW && dist == HIGH){encode_2++;}*/
  //Left
  if(direction ==HIGH && dist == HIGH){encode_2++;}
  if(direction ==LOW && dist == HIGH){encode_2--;}

}


void setup() {
  pinMode(CLOCK, OUTPUT);
  pinMode(DATA, OUTPUT);
  pinMode(LATCH, OUTPUT);
  pinMode(DIR, OUTPUT);
  pinMode(PWM_PIN, OUTPUT);
  pinMode(SIG, OUTPUT);

  clock_low();
  data_low();
  latch_low();
  digitalWrite(DIR, LOW);
  digitalWrite(PWM_PIN, LOW);
  digitalWrite(SIG, LOW);

  zeroFrame();
  sendFrame();

  Serial.begin(1000000);
  for (int y = 0; y < Y; y++) {
    for (int x = 0; x < X; x++) {
      frame_info[x][y] = 0;
    }
  }

  uint16_t divider  = 1;
  uint16_t compare = 0;
  tc_clock_prescaler prescaler = TC_CLOCK_PRESCALER_DIV1;

  divider = 1;
  prescaler = TC_CLOCK_PRESCALER_DIV1;
  compare = 48000000/timer_freq;
  
  zerotimer.enable(false);
  zerotimer.configure(prescaler,       // prescaler
          TC_COUNTER_SIZE_16BIT,       // bit width of timer/counter
          TC_WAVE_GENERATION_MATCH_PWM // frequency or PWM mode
          );

  zerotimer.setCompare(0, compare);
  zerotimer.setCallback(true, TC_CALLBACK_CC_CHANNEL0, TimerHandler);
  zerotimer.enable(true);

  //Initialize PWM duty cycle
  PWM_Instance = new SAMD_PWM(PWM_PIN, pwm_freq, dutyCycle);
  if(PWM_Instance){
    PWM_Instance->setPWM(PWM_PIN, pwm_freq, 0);
  }


  pinMode(dir_1, INPUT);
  pinMode(dis_1, INPUT);
  pinMode(dir_2, INPUT);
  pinMode(dis_2, INPUT);
  digitalWrite(dis_1, LOW);
  digitalWrite(dir_1, LOW);
  digitalWrite(dis_2, LOW);
  digitalWrite(dir_2, LOW);
  attachInterrupt(digitalPinToInterrupt(dis_1), handler_1, CHANGE);
  attachInterrupt(digitalPinToInterrupt(dis_2), handler_2, CHANGE);
}

void printFrame(){
  for(int x=0; x<X; x++){
    for(int y=0; y<Y; y++){
      Serial.print(frame_info[x][y]);
    }
    //Serial.println("");
  }
}

void vib_half(int a, int b){
  for(int x = 0; x<X; x++){
    for(int y=0; y<Y; y++){
      if(y< 9){frame[x][y] = a;}
      else{frame[x][y] =b;}
    }
  }
}

byte receive[108];
byte tempBuffer[108];
int idx = 0;
int ind = 0;
byte distance[] = {0, 0, 255}; //255 is seperator
int sum = 0;
void loop(){
  for(int number = 0; number < 5; number++){
    if(number == 0){
      sum +=1;
      int diff_1 = encode_1 - old_1;
      //Serial.print(diff_1);
      if(diff_1 > 127){diff_1 = 127;}
      else if(diff_1 < -127){diff_1 = -127; }
      if( diff_1>= 0){distance[0] = diff_1; }
      else{distance[0] = 127 - diff_1;}
      //Serial.print(" ");
      int diff_2 = encode_2 - old_2;
      //Serial.println(diff_2);
      if(diff_2 > 127){diff_2 = 127;}
      else if(diff_2 < -127){diff_2 = -127; }
      if( diff_2>= 0){distance[1] = diff_2; }
      else{distance[1] = 127 - diff_2;}
      
      /*Serial.print(encode_1);
      Serial.print(" ");
      Serial.println(encode_2);*/
      Serial.write(distance, sizeof(distance));
      Serial.flush();
      old_1 = encode_1;
      old_2 = encode_2;
    }
    unsigned long start_time = micros();
    latch();
    inter = 0;

    //zerotimer.enable(true);
    int available = Serial.available();
    //Serial.println(available);

    if(Serial.available() >0){

      Serial.readBytes((char*)tempBuffer, available);
      int final_idx = idx + available;
      if(final_idx <=108){
        for(int i=idx; i<final_idx; i++){
          receive[i] = tempBuffer[i- idx];
        }
        idx = final_idx;
        if(final_idx==108){
          idx=0;
          bytepattern(receive);
        }
      }
      else{
        for(int i = idx; i < 108; i++){
        receive[i] = tempBuffer[i - idx];
        }
        bytepattern(receive);
        //digitalWrite(SIG, LOW);
        //printFrame();
        final_idx = final_idx - 108;
        for(int i = 0; i< idx; i++ ){
          receive[i] = tempBuffer[final_idx + i];
        }
      }
    }

    if(number ==0){
      bytepattern(receive);
      sendFrame();
    }

    //pattern_now(number);
    //sendFrame();

    while(micros() < start_time + 4000);
    //TimerTc3.stop();
    //zerotimer.enable(false);
  }
}