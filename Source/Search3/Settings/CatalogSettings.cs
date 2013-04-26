﻿using System;
using System.ComponentModel;
using HydroDesktop.Common;

namespace Search3.Settings
{
    /// <summary>
    /// Catalog settings 
    /// </summary>
    public class CatalogSettings : ObservableObject<CatalogSettings>
    {
        /// <summary>
        /// Gets default url of HisCentral
        /// </summary>
        public static string HISCENTRAL_URL_1
        {
            get { return "http://hiscentral.cuahsi.org/webservices/hiscentral_1_1.asmx"; }
        }

        /// <summary>
        /// Gets secondary url of HisCentral
        /// </summary>
        public static string HISCENTRAL_URL_2
        {
            get { return "http://water.sdsc.edu/hiscentral/webservices/hiscentral.asmx"; }
        }
    

        private string _hisCentralUrl;
        public string HISCentralUrl
        {
            get { return _hisCentralUrl; }
            set
            {
                _hisCentralUrl = value;
                NotifyPropertyChanged(() => HISCentralUrl);
            }
        }

        private TypeOfCatalog _typeOfCatalog;
        public TypeOfCatalog TypeOfCatalog
        {
            get { return _typeOfCatalog; }
            set
            {
                _typeOfCatalog = value;
                NotifyPropertyChanged(() => TypeOfCatalog);
            }
        }

        public CatalogSettings()
        {
            TypeOfCatalog = TypeOfCatalog.HisCentral;
            HISCentralUrl = HISCENTRAL_URL_1;
        }

        /// <summary>
        /// Create deep copy of current instance.
        /// </summary>
        /// <returns>Deep copy.</returns>
        public CatalogSettings Copy()
        {
            var result = new CatalogSettings();
            result.Copy(this);
            return result;

        }

        /// <summary>
        /// Create deep from source into current instance.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>must be not null.</exception>
        public void Copy(CatalogSettings source)
        {
            if (source == null) throw new ArgumentNullException("source");

            HISCentralUrl = source.HISCentralUrl;
            TypeOfCatalog = source.TypeOfCatalog;
        }
    }

    public enum TypeOfCatalog
    {
        [Description("HIS Central")]
        HisCentral,
        [Description("Local Metadata Cache")]
        LocalMetadataCache
    }
}
