using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataObjects
{
    class Marriage
    {
        public int husbandId;
        public int wifeId;
        public int marriageMonth;
        public int marriageDay;
        public int marriageYear;
        public int annulledYear;
        public int divorcedYear;

        public Marriage(int husbandId, int wifeId, int marriageMonth, int marriageDay, int marriageYear, int annulledYear, int divorcedYear)
        {
            this.husbandId = husbandId;
            this.wifeId = wifeId;
            this.marriageMonth = marriageMonth;
            this.marriageDay = marriageDay;
            this.marriageYear = marriageYear;
            this.annulledYear = annulledYear;
            this.divorcedYear = divorcedYear;
        }
    }
}
