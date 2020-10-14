using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet
{
    public static class Constants
    {
        public static readonly double DEGREES_PER_POP = 0.04;
        public static readonly double DEGREES_PER_EXCESS_MWH = 0.75;
        public static readonly double ADJUST_ENERGY_COST = 150;
        public static readonly double LOW_HEALTH = 40;
        public static readonly double QUEUE_MAX_HAPPINESS = 20;
        public static readonly double LONG_QUEUE = 15;
        public static readonly double QUEUE_TICK_THREASHOLD = 12;
        public static readonly double CO2_PER_POP = 0.03;
        public static readonly double LOW_HAPPINESS = 0.3;


        //Map constants
        public static readonly double AVG_DECAY_INCREASE = 0.6;
        public static readonly double TARGET_END_POP_COUNT = 300;
        public static readonly double AVG_POP_HAPPINESS = 1.3;
        public static readonly double AVG_POP_HAPPINESS_PRECENT = 1;
        public static readonly double AVG_EFFECT_HAPPINESS_INCREASE = 0.15;

    }
}
