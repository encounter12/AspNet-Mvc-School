﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class AcademicYear
    {
        private IList<Grade> grades;

        private IList<SchoolTheme> schoolThemes;

        public AcademicYear()
        {
            this.grades = new List<Grade>();
            this.schoolThemes = new List<SchoolTheme>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public virtual IList<Grade> Grades
        {
            get { return this.grades; }
            set { this.grades = value; }
        }

        public virtual IList<SchoolTheme> SchoolThemes
        {
            get { return this.schoolThemes; }
            set { this.schoolThemes = value; }
        }
    }
}