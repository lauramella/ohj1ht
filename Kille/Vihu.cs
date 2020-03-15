using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;



public class Vihu : PhysicsObject
{
    public IntMeter HP;

    public Vihu(double leveys, double korkeus, int elamapisteet)
        :base(leveys, korkeus)
    {
        HP = new IntMeter(elamapisteet, 0, elamapisteet);
        HP.LowerLimit += delegate () { this.Destroy(); };
    }

}

