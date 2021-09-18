using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataObjects
{
    class Parentage
    {
        public int familyId;
        public int fatherId;
        public int motherId;
        public int childId;
        public ChildRelationshipType relationToFather;
        public ChildRelationshipType relationToMother;
  
        public Parentage(int familyId, int fatherId, int motherId, int childId, ChildRelationshipType relationToFather, ChildRelationshipType relationToMother)
        {
            this.familyId = familyId;
            this.fatherId = fatherId;
            this.motherId = motherId;
            this.childId = childId;
            this.relationToFather = relationToFather;
            this.relationToMother = relationToMother;
        }
    }
}
