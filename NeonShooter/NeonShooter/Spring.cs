using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace NeonShooter
{
    public class Spring
    {
        public PointMass End1;
        public PointMass End2;
        public float TargetLength;
        public float Stiffness;
        public float Damping;
        public Spring(PointMass end1, PointMass end2, float stiffness, float damping)
        {
            End1 = end1;
            End2 = end2;
            Stiffness = stiffness;
            Damping = damping;
            TargetLength = Vector3.Distance(end1.Position, end2.Position)*0.95f;
        }

        public void Update()
        {
            var x = End1.Position - End2.Position;
            float length = x.Length();
            //set to spring can only pull, cant push
            if (length <= TargetLength)
            {
                return;
            }
            x = x/length*(length - TargetLength);
            var dv = End2.Velocity - End1.Velocity;
            var force = Stiffness*x - dv*Damping;
            End1.ApplyForce(-force);
            End2.ApplyForce(force);
        }
    }
}
