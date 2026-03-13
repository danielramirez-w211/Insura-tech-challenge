using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        private int _version {  get; set; }
        public int Version => _version;

        protected Entity() {
            Id = Guid.NewGuid();
            CreateAt = DateTime.UtcNow;
        }

        public void IncrementVersion () => _version++;

        protected void MarkAsUpdated() => UpdateAt = DateTime.UtcNow;

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }
        public override int GetHashCode () => Id.GetHashCode();

    }
}
