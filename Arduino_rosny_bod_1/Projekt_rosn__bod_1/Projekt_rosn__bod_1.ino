//pinout
#define PWM_BRIDGE_2 13// Ovládání H-mostu pohyb vpravo (Zahřívání)
#define IN_BRIDGE_ALERT1_Right 10 //Hlášení unikajícího proudu
#define ENABLE_BRIDGE_RIGHT 9 // řístup k paměti enable Right mosfetů!
#define ENABLE_BRIDGE_LEFT 8 // řístup k paměti enable Left mosfetů!
#define IN_BRIDGE_ALERT2_left 7 //-//
#define PWM_BRIDGE_1 6// Ovládání H-mostu pohyb vlevo chkazení
#define CS 5 // select pro Max31865 Enable pin zařízení, komunikace řešena přes SPI
#define PWM_FAN 0 //ventilátor
#define IN_SENZOR_SVETLA A0
#define IN_SENZOR_TEPLOTY_CHLADICE A1
#define IN_SENZOR_AMPERY A3
/////////////////////////////////////////////////////////////////////
//MAX 31865
// The 'nominal' 0-degrees-C resistance of the sensor
// 100.0 for PT100, 1000.0 for PT1000
#define RNOMINAL  100.0
// The value of the Rref resistor. Use 430.0 for PT100 and 4300.0 for PT1000
#define RREF      430.0
#define BMP280_ADRESA (0x76) // nastavení adresy senzoru
#define BaudRate 115200 //rychlost komunikace
/////////////////////////////////////////////////////////////////////
// připojení potřebných knihoven
#include <Wire.h>
#include <BMP280_DEV.h>
#include <Adafruit_MAX31865.h> //customizace knihovny viz https://forum.arduino.cc/t/max31865-library-removing-delay-changing-to-non-blocking-method/672925/5
#include <pt100rtd.h>

Adafruit_MAX31865 thermo = Adafruit_MAX31865(CS);
BMP280_DEV bmp; // inicializace senzoru BMP  / I2C



//WRITEBUFFER [7]
//CCCC;INT;FLOAT;FLOAT;INT;INT;FLOAT
// Analog read internal 32K resistor , 10bit sběrnice, vstupní impedance input pin modu je 100Mohmů
//1 – 4 CHAR (od 0 do 1) - BEZPEČNOSTNÍ BITY - Rezervovány pro použití relé / dalších prvků
//5 INT - VYSTUP SENZORU SVĚTLA
//6 FLOAT - ENV SENZOR TEPLA
//7 FLOAT - ENV SENZOR TLAKU
//8 INT - VÝSTUP ZE SENZORU PROUDU Ampéry 0 A == 2.5V, 100mV/A -> 0.047 / krok
//9 INT - VÝSTUP Z LINEARIZOVANÉHO TNC SENZORU
//10 FLOAT - VÝSTUP Z 4 VODIČOVÉ PT 100


//READBUFFER [11]
//STRUKTURA CCCCCCCC;UINT8;UINT8;UINT8

//0.C - 4C  - BEZPEČNOSTNÍ BITY - V SOUČASNÉ DOBĚ VYŘAZENÉ
//5 C - UPDATE CYKLUS BMP280
//6 C - UPDATE CYKLUS MAX31865
//7 C - UPDATE CYKLUS PWM ŘÍZENÝCH MODULŮ

//2.  - PWM velikost Hbridge Left
//3.  - PWM velikost Hbridge Right
//4.  - PWM velikost Hbridge FAN

  //////////////////////////////////////STATE SWITCHING
  // 00000000;000;000;000 - -+ 2400 mikrosekund - 2.4 ms
  // 00000100;000;000;000 - -+ 12800 mikrosekund - 12,8 ms
  // 00000010;000;000;000 - -+ 78300 mikrosekund - 78.3 ms // neplatí, již asynchronně -75 ms
  // 00000000;255;000;000 - -+ 4700 mikrosekund - 4.7 ms ( 2 ms čekání na ustálení mosfetu)
  // 00000000;000;000;255 - -+ 2600 mikrosekund - 2.6 ms 
  // 11111111;255;255;255 - +- 92500 mikrosekund - 92.5 ms // neplatí, již asynchronně -75 ms

int pocetbytu = 0;
bool debug = false;
bool safety_current_left_on = false;
bool safety_current_right_on = false;
bool safety_relay_1_on = false;
bool safety_relay_2_on = false;
bool old1 = false;
bool old2 = false;
uint16_t rtd = 0;
int korekce = 32; // konstanta s korekcí měření v hPa
pt100rtd PT100 = pt100rtd() ;

void setup() {
  delay(5000); // POČKEJ, NEŽLI SE KOMPONENTY NABYJÍ, AŤ NEDOJDE K SELHÁNÍ ŘÍZENÍ

  pinMode(ENABLE_BRIDGE_RIGHT, OUTPUT);
  pinMode(ENABLE_BRIDGE_LEFT, OUTPUT);

  pinMode(PWM_BRIDGE_1, OUTPUT);
  pinMode(PWM_BRIDGE_2, OUTPUT);

  digitalWrite(ENABLE_BRIDGE_RIGHT, HIGH); //PO STARTU RESETUJ Hbridge
  digitalWrite(PWM_BRIDGE_1, LOW);
  digitalWrite(ENABLE_BRIDGE_RIGHT, LOW);

  digitalWrite(ENABLE_BRIDGE_LEFT, HIGH);
  digitalWrite(PWM_BRIDGE_2, LOW);
  digitalWrite(ENABLE_BRIDGE_LEFT, LOW);

  
  thermo.begin(MAX31865_4WIRE);
  Serial.begin(BaudRate);
  Serial.setTimeout(20);
  bmp.begin(BMP280_I2C_ALT_ADDR);
  bmp.setTimeStandby(TIME_STANDBY_62MS);
  bmp.startNormalConversion(); 
  //  readtemp();

}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// ZPRACOVÁNÍ READBUFFERU
String Internaltemp = "00.00";
String WRITEBUFFER; // prostor pro export dat
String  READBUFFER; //prostor pro inport dat
byte  READBUFFERSEGMENTED[11] = {0}; //prostor pro analýzu dat
float ENV_TEMP;
float ENV_PRES;
uint16_t ohmsx100 ;
uint32_t dummy ;

void loop() {
    //timer(true);
  //Serial.println((char)B0000001);
  //Serial.println("Ready for com");//debug

  bool input = serialcomread(READBUFFER);
  //Serial.println(READBUFFER + "MAIN"); //debug
  if (input) {
    readanalyzer(READBUFFER, READBUFFERSEGMENTED);
  }
  ////////////////////////////////////////////////OVLÁDÁNÍ CURRENT ENABLE BRIDGE


   
  if (READBUFFERSEGMENTED[0] == 1) { //Když počítač vyšle signál HBRIDGE CURRENT LEFT ENABLE, nastav výstup ENABLE_BRIDGE_RIGHT NA 1
 
    WRITEBUFFER = String('1');
    // Serial.println("readbuffer[0] - 8");//debug
  }
  else {
  //  safety_current_left_on = false;
    WRITEBUFFER = String('0');
  };

  if (READBUFFERSEGMENTED[1] == 1) { //Když počítač vyšle signál HBRIDGE CURRENT RIGHT ENABLE, nastav výstup ENABLE_BRIDGE_LEFT NA 1

    WRITEBUFFER = WRITEBUFFER + String('1');
    //   Serial.println("readbuffer[0] - 7");//debug
  }
  else {
 //   safety_current_right_on = false;
    WRITEBUFFER = WRITEBUFFER + String('0');
  }; 
  ////////////////////////////////////////////////OVLÁDÁNÍ RELAY

  if (READBUFFERSEGMENTED[2] == 1) { //Když počítač vyšle signál State relay 1, nastav výstup OUT_RELE_1 NA 1
    safety_relay_1_on = true;
    WRITEBUFFER = WRITEBUFFER +  String('1');
    //   Serial.println("readbuffer[0] - 6");//debug
  }
  else {
    safety_relay_1_on = false;
    WRITEBUFFER = WRITEBUFFER +  String('0');
  };


  if (READBUFFERSEGMENTED[3] == 1) { //Když počítač vyšle signál State relay 2, nastav výstup OUT_RELE_2 NA 1
    safety_relay_2_on = true;
    WRITEBUFFER = WRITEBUFFER + String('1') + String(';');;
    //   Serial.println("readbuffer[0] - 5");//debug
  }
  else {
    safety_relay_2_on = false;
    WRITEBUFFER = WRITEBUFFER + String('0') + String(';');;
  };
  if (READBUFFERSEGMENTED[4] == 1) { //Když počítač vyšle signál State relay 2, nastav výstup OUT_RELE_2 NA 1
    debug = 1;
  }
  else {
    debug = 0;
  }
  if (READBUFFERSEGMENTED[5] == 1) { // čtení teploty má 100 ms delay
    readtemp();
  };
  if (READBUFFERSEGMENTED[6] == 1) {
    if (thermo.readRTDAsync(rtd)) {
        dummy = ((uint32_t)(rtd << 1)) * 100 * ((uint32_t) floor(RREF)) ;
  dummy >>= 16 ;
  ohmsx100 = (uint16_t) (dummy & 0xFFFF) ;
  
  Internaltemp = PT100.celsius(ohmsx100); 
    }
   // Internaltemp = thermo.temperature(RNOMINAL, RREF);
  };
  if (READBUFFERSEGMENTED[7] == 1) {

  };
  /////////////////////////////////////////////// PWM řízení - 490 Hz (piny 3 a 11: 980 Hz) // pozn - při připojení pinů 3 a 11 deska zamrzne
  if (heartbeat(READBUFFERSEGMENTED[0],READBUFFERSEGMENTED[1])){
  Hbridge_control(READBUFFERSEGMENTED[8], READBUFFERSEGMENTED[9], safety_current_left_on, safety_current_right_on);
  Fan_control(READBUFFERSEGMENTED[10]); // PWM velikost Hbridge FAN
  }
  else {
  Hbridge_control(1,1,safety_current_left_on, safety_current_right_on);
  }
  //////////////////////////////////////////////////
  /////////////////////////////////////////////////////////////////////////////////////////////////// REPORT SEGMENT


  /////////////////////////////// nacti hodnoty ze senzoru světla
  WRITEBUFFER = WRITEBUFFER + String(analogRead(IN_SENZOR_SVETLA)) + String(';');
  ////////////////////////////// nacti hodnoty ze senzoru prostředí
  WRITEBUFFER = WRITEBUFFER + String (ENV_TEMP, 6) + String(';'); //zápis teploty
  WRITEBUFFER = WRITEBUFFER + String (ENV_PRES, 6) + String(';'); //zápis tlaku
  WRITEBUFFER = WRITEBUFFER + String(analogRead(IN_SENZOR_AMPERY)) + String(';'); // zápis ampér
  WRITEBUFFER = WRITEBUFFER + String(analogRead(IN_SENZOR_TEPLOTY_CHLADICE)) + String(';');
  WRITEBUFFER = WRITEBUFFER + Internaltemp + String('\n');
  serialcomwrite(WRITEBUFFER);
  //timer(false);
}

///////////////////////////////////////////////////////////////////////////////////////////////////fce


void Fan_control (byte FanPWMrange) {
  static byte FanPWMold = 0;
  if (FanPWMold != FanPWMrange) {
    analogWrite(PWM_FAN, FanPWMrange);
    FanPWMold = FanPWMrange;
  }
}
void Hbridge_control (byte RightPWM, byte LeftPWM, bool &Right_side_current_ALARM, bool &Left_side_current_ALARM) {
  //komentář
  //Enable piny blokují pouze zápis do paměti hbridge
  //K regulaci je proto nutné použít pouze PWM řízení

  int alert_check = digitalRead(IN_BRIDGE_ALERT1_Right);
  int alert_check2 = digitalRead(IN_BRIDGE_ALERT2_left);

  static byte RightPWMold = 0;
  static byte LeftPWMold = 0;
  bool changed = false; // proměnná řídící přístupy do paměti hbridge

  bool rightpwmchange = RightPWMold != RightPWM;//detekce změn nastavení
  bool leftpwmchange = LeftPWMold != LeftPWM;

      if (leftpwmchange) { // pohyb "vlevo" chlazení
      digitalWrite(ENABLE_BRIDGE_LEFT, HIGH); //Pokud dojde ke změně nastavení Hbridge, nastav paměťový vstup na zapnuto a zapiš proměnnou
      digitalWrite(ENABLE_BRIDGE_RIGHT, HIGH);
      delay(2); // PWM velikost Hbridge left
      analogWrite(PWM_BRIDGE_2, LeftPWM);
      analogWrite(PWM_BRIDGE_1, RightPWM);
      LeftPWMold =LeftPWM;
      //changed = true;
    }
        if (rightpwmchange) { // pohyb "vlevo" chlazení
      digitalWrite(ENABLE_BRIDGE_LEFT, HIGH); //Pokud dojde ke změně nastavení Hbridge, nastav paměťový vstup na zapnuto a zapiš proměnnou
      digitalWrite(ENABLE_BRIDGE_RIGHT, HIGH);
      delay(2); // PWM velikost Hbridge left
      analogWrite(PWM_BRIDGE_2, LeftPWM);
      analogWrite(PWM_BRIDGE_1, RightPWM);
      RightPWMold = RightPWM;
      //changed = true;
    }
  
}

void MaxFault() { //debug
  uint8_t fault = thermo.readFault();
  if (fault) {
    Serial.print("Fault 0x"); Serial.println(fault, HEX);
    if (fault & MAX31865_FAULT_HIGHTHRESH) {
      Serial.println("RTD High Threshold");
    }
    if (fault & MAX31865_FAULT_LOWTHRESH) {
      Serial.println("RTD Low Threshold");
    }
    if (fault & MAX31865_FAULT_REFINLOW) {
      Serial.println("REFIN- > 0.85 x Bias");
    }
    if (fault & MAX31865_FAULT_REFINHIGH) {
      Serial.println("REFIN- < 0.85 x Bias - FORCE- open");
    }
    if (fault & MAX31865_FAULT_RTDINLOW) {
      Serial.println("RTDIN- < 0.85 x Bias - FORCE- open");
    }
    if (fault & MAX31865_FAULT_OVUV) {
      Serial.println("Under/Over voltage");
    }
    thermo.clearFault();
  }
  Serial.println();
  delay(1000);
}

void readtemp() {
  if(bmp.getTempPres(ENV_TEMP,ENV_PRES)){
  ENV_PRES = ENV_PRES + korekce;
  //Serial.print(ENV_TEMP);
  //Serial.print(ENV_PRES);
  }
}


bool serialcomread (String &reading) {
  if (Serial.available()) {
    reading = Serial.readString();
    return true;
  }
  else {
    return false;
  }
}

void readanalyzer (String &input, byte report []) // float je připravený pro případ použití interního regulátoru
{

  //Serial.println(input + "Read analyzer"); //debug
  char reading[22] = {0};
  input.toCharArray(reading, 21);
  // 11111111;255;255;255
  report[0] = (byte)(reading[0] - '0'); //safety bit1 (Hbridgeleftok) uz neni treba
  report[1] = (byte)(reading[1] - '0'); //safety bit2 (Hbridgerightok)
  report[2] = (byte)(reading[2] - '0'); //safety bit3 (1.Relay)
  report[3] = (byte)(reading[3] - '0'); //safety bit4 (2.Relay)
  report[4] = (byte)(reading[4] - '0'); //debug on
  report[5] = (byte)(reading[5] - '0'); //active stage 1
  report[6] = (byte)(reading[6] - '0'); //active stage 2
  report[7] = (byte)(reading[7] - '0'); //active stage 3
  report[8] = (byte)((reading[9] - '0') * 100 + (reading[10] - '0') * 10 + (reading[11] - '0')); //pwm hbridgeleft
  report[9] = (byte)((reading[13] - '0') * 100 + (reading[14] - '0') * 10 + (reading[15] - '0')); //pwm hbridgeright
  report[10] = (byte)((reading[17] - '0') * 100 + (reading[18] - '0') * 10 + (reading[19] - '0')); //pwm fan
  //  Serial.println(String(report[10]));

}

void convertinttochar (int cislo, char pismeno1, char pismeno2) { ///// debug!
  pismeno1 = cislo / 255;
  pismeno2 = cislo % 255;
}

void convertfloattochar (float cislo, char pismeno1, char pismeno2) { ///// debug!
  cislo = (cislo * 100);
  pismeno1 = (int)cislo / 255;
  pismeno2 = (int)cislo % 255;
}

float mapfloat (int x, int y, int z , int a, int b) {
  return (x - y) * (b - a) / (z - y) + a;
}

void serialcomwrite (String writing) {
  if (Serial.availableForWrite()) {
    Serial.print(writing);
  }
}
bool heartbeat (bool bitone, bool bittwo){ // pokud nedojde ke změně arraye po 1s tak vypni peltier
int static watchdog = 0;
bool bitarray[2] = {bitone, bittwo};
bool static bitarrayold[2] = {0,0};

if (bitarray != bitarrayold){
watchdog = 0;
}
else {
watchdog = watchdog +1;
}

if (watchdog<60){
return true;
}
else{
return false;
}


}
void timer (bool casaktivni){
  static unsigned long time = 0;
  if (casaktivni){
    time = micros();
    }
   else {
    Serial.println(micros()-time);
    }
  }
