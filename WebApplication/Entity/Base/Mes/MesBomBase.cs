using System;
using System.Collections;
using System.Collections.Generic;

//TODO: Add other using statements here

namespace com.Sconit.Entity.Mes
{
    [Serializable]
    public abstract class MesBomBase : EntityBase
    {
        #region O/R Mapping Properties
		
		private string _code;
		public string Code
		{
			get
			{
				return _code;
			}
			set
			{
				_code = value;
			}
		}
		private string _description;
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}
		private com.Sconit.Entity.MasterData.Uom _uom;
		public com.Sconit.Entity.MasterData.Uom Uom
		{
			get
			{
				return _uom;
			}
			set
			{
				_uom = value;
			}
		}
		private com.Sconit.Entity.MasterData.Region _region;
		public com.Sconit.Entity.MasterData.Region Region
		{
			get
			{
				return _region;
			}
			set
			{
				_region = value;
			}
		}
		private Boolean _isActive;
		public Boolean IsActive
		{
			get
			{
				return _isActive;
			}
			set
			{
				_isActive = value;
			}
		}
        private IList<MesBomDetail> _bomDetails;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IList<MesBomDetail> BomDetails
        {
            get
            {
                return _bomDetails;
            }
            set
            {
                _bomDetails = value;
            }
        }
        private Boolean _transferFlag;
        public Boolean TransferFlag
        {
            get
            {
                return _transferFlag;
            }
            set
            {
                _transferFlag = value;
            }
        }
        #endregion

		public override int GetHashCode()
        {
			if (Code != null)
            {
                return Code.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            MesBomBase another = obj as MesBomBase;

            if (another == null)
            {
                return false;
            }
            else
            {
            	return (this.Code == another.Code);
            }
        } 
    }
	
}
