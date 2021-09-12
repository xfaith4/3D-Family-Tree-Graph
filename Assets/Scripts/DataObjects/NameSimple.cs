using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataBase.DBObjects
{
    class NameSimple
    {
        public int _ownerId;
        public Char _sex;
        public string _given;
        public string _surname;
        public int _birthYear;
        public int _deathYear;

        public NameSimple(int ownerId, Char sex, string given, string surname, int birthYear, int deathYear)
        {
            _ownerId = ownerId;
            _sex = sex;
            _given = given;
            _surname = surname;
            _birthYear = birthYear;
            _deathYear = deathYear;
        }


        public static NameSimple getFakeName()
        {
            return new NameSimple(0, 'M', "Given Name", "Surname", 1903, 1987);
        }
    }
}