﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotSpatial.Controls;
using DotSpatial.Projections;
using HydroDesktop.Interfaces;
using System.ServiceModel.Description;
using DotSpatial.Data;
using DotSpatial.Topology;

namespace FacetedSearch
{
    /// <summary>
    /// The user control added to the main window by this plugin
    /// </summary>
    public partial class FacetedSearchControl : UserControl
    {        
        public string BaseMapPath;                                          // location of BaseMap for Map display/query
        public Boolean SpatialTemporalCommitted = false;                    // tracks state of the form => determines whether the user is ready to use the Faceted Search features.
        public List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> TopLevelFacets;    // contains the XML representation of the entire searchable ontology         
        public List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> SelectedFacets;    // contains a record of what facets have been selected for the existing search. Equal to summing all selections from each existing SearchFacetSpecifier object.
      
        public DateTime BeginDateTime;                                      // historical boundary of search temporal constraints
        public DateTime EndDateTime;                                        // modern boundary of search temporal constraints
        public Boolean IncludeSpatialResults = true;                       // boolean variable designating whether an additional db transaction is desired for including additional metadata properties (for now, just Lat/Long) in ontology-pruning service response.
        
        private AppManager App;                                             // the AppManager extension through which this control interacts with HydroDesktop                
        public DotSpatial.Data.Extent SrchExt;                              // Extents of last spatial search       
        public static int ShapeFileThreshold = 25000;                       // Seach results layers that contain more than this number of records create a shapefile create a shapefile inside the CurrentProjectDirectory        


        private static string FacetedShapeFileName = "FacetedSearchResults.shp";
        private static string FacetedShapeLayerName = "FacetedSearchResults.shp";

        #region Methods
        public FacetedSearchControl(AppManager a)
        {
            InitializeComponent();
            App = a;            
            LoadTotalFacetCollection();
            ResetInterface();
        }
        
        /// <summary>
        /// Takes spatial temporal constraints from main form
        /// </summary>
        /// <param name="SearchRegion"></param>
        /// <param name="sTime"></param>
        /// <param name="eTime"></param>
        public void SetSearchParameters(Extent SearchRegion, DateTime sTime, DateTime eTime)
        {
            SrchExt = SearchRegion;
            BeginDateTime = sTime;
            EndDateTime = eTime;
        }        

        /// <summary>
        /// Called on client initialization. Loads the entire set of possible ontology terms to client-side memory. All mechanisms to display "what's left" will prune this collection.
        /// </summary>
        public void LoadTotalFacetCollection()
        {
            try
            {
                using (FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl = new FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient())
                {
                    bool configed = false;
                    try
                    {
                        ConfigureCUAHSIChannelFactory(cl);
                        configed = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Error configuring channel factory with message {0}, inner ex: {1}", ex.Message, ex.InnerException));
                    }
                    try
                    {
                        if (configed) 
                        {
                            TopLevelFacets = cl.GetAllTypedOntologyElements().ToList();
                        }                        
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(string.Format("Error refreshing ontology from metadata source with message {0}, {1}.", e.Message, e.InnerException));
                    }
                }            
            }
            catch (Exception exc)
            {
#if DEBUG
                MessageBox.Show("Error connecting to server. Please verify you have access to the internet and try again. Message: " + exc.Message);    
#else
                  MessageBox.Show("Error connecting to server. Please verify you have access to the internet and try again.");          
#endif
            }                
        }

        /// <summary>
        /// Sets the MaxItemsInObjectGraph behavior to permit sufficiently-large transactions for faceted search
        /// </summary>
        /// <param name="cl"></param>
        private static void ConfigureCUAHSIChannelFactory(FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl)
        {
            foreach (var operation in cl.ChannelFactory.Endpoint.Contract.Operations)
            {
                var behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
                if (behavior != null)
                {
                    behavior.MaxItemsInObjectGraph = 2147483647;
                }
               
            }
        }
     
        /// <summary>
        /// Checks that spatial and temporal requirements for the faceted search have been met.
        /// Hits the FacetedSearch.CUAHSIFacetedSearch web service to determine the global facet set available for querying.
        /// Enables the FacetFlow object
        /// Creates the first SearchFacetSpecifier object.
        /// Stores the global facet set for the spatial-temporal constraints specified in the first SearchFacetSpecifier object.
        /// Stores the global facet set in the public form-level object TopLevelFacets for go-back purposes. [CONSIDER DEPRECATING IF NOT USEFUL]
        /// </summary>
        public void InitializeFacetedSearch()
        {
            List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> MyRemainingFacets = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
            if (InputsSufficientForFacetedSearch())
            {
                SpatialTemporalCommitted = true;
                RemoveDownStreamSearchFacetSpecifiers(0);        // remove all searchfacetspecifers
                ResetFacetFlowPanel();
                using (FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl = new FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient())
                {
                    ConfigureCUAHSIChannelFactory(cl);                    
                    try
                    {
                        FacetedSearch.CUAHSIFacetedSearch.OntologyEnvelope env = cl.GetTypedOntologyElementsGivenConstraints(SelectedFacets, BeginDateTime, EndDateTime, SrchExt.MinY, SrchExt.MaxY, SrchExt.MinX, SrchExt.MaxX, false);
                        MyRemainingFacets = env.OntologyElements;
                        FacetFlowPanel.Enabled = true;
                        FacetFlowPanel.Controls.Add(new SearchFacetSpecifier(this, TopLevelFacets, SelectedFacets, MyRemainingFacets, FacetFlowPanel.Controls.Count));                        
                    }
                    catch (Exception e)
                    {
#if DEBUG                        
                        MessageBox.Show(string.Format("Error in transit layer with message {0} and inner exception {1}", e.Message, e.InnerException));
#else
                    MessageBox.Show("Error in transit layer. Please try again.");
#endif
                    }
                }

                ResizeSearchSpecifiers();
            }
            else
            {
                MessageBox.Show("Please select a spatial region to bound your search prior to initiating faceted search");                
            }
        }
        
        /// <summary>
        /// Input validation to determine if conditions for faceted search have occurred.
        /// Reads Map1 object to set global selected-shapes variable "SelectedShapes"
        /// </summary>
        /// <returns></returns>
        public Boolean InputsSufficientForFacetedSearch()
        {

            return (SrchExt != null);

            /*
            object areaParameter = null;
            if (App.AreaSettings.Polygons != null)
            {
                var polyFs = settings.AreaSettings.Polygons;
                string esri = Resources.wgs_84_esri_string;
                var wgs84 = ProjectionInfo.FromEsriString(esri);

                //reproject the selected polygons to WGS1984         
                polyFs.Reproject(wgs84);

                //the list of selected polygons passed in to the search function
                var selectedPolygons = Enumerable.ToList(polyFs.Features);

                areaParameter = selectedPolygons;
            }
            else if (settings.AreaSettings.AreaRectangle != null)
            {
                areaParameter = settings.AreaSettings.AreaRectangle;
            }
            return true;
            */

        }

        /// <summary>
        /// Clears form and search-generated objects, resets UI to prepare for search.
        /// </summary>
        public void ResetInterface()
        {
            #region Reset Map and Map-affiliated parameters
            SpatialTemporalCommitted = false;
            // map1.ZoomToMaxExtent();
            // map1.ClearSelection();
            SrchExt = new DotSpatial.Data.Extent();
            #endregion

            #region Reset default DateTime values
            // BeginTimePicker.Value = DateTime.Parse("1/1/1911 1:00 AM");
            // EndTimePicker.Value = DateTime.Now;
            #endregion

            #region Clear and Disable Faceted Search interface
            ResetFacetFlowPanel();
            #endregion                                                
        }

        /// <summary>
        /// Resets faceted search without resetting user-defined spatial-temporal data
        /// </summary>
        private void ResetFacetFlowPanel()
        {
            FacetFlowPanel.Controls.Clear();
            FacetFlowPanel.Enabled = false;
            SelectedFacets = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
        }

        /// <summary>
        /// Provides surface for SearchFacetSpecifiers to specify the next facet of the search, executed by spawning a new SearchFacetSpecifier.
        /// Uses CUAHSI Faceted Search webservices to determine what options are available, given the user's selections.
        /// Regenerates spatial-temporal options, then submits faceted search
        /// </summary>
        /// <param name="AllSelectedFacets"></param>
        public void InvokeNextButton(List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> AllSelectedFacets, int MyIndex)
        {
            SelectedFacets = AllSelectedFacets;
            List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> MyRemainingFacets = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
            if (InputsSufficientForFacetedSearch())
            {
                try
                {
                    RemoveDownStreamSearchFacetSpecifiers(MyIndex + 1);
                    SpatialTemporalCommitted = true;
                    // SrchExt = SelectedShapes.Envelope.ToExtent();
                    using (FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl = new FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient())
                    {
                        ConfigureCUAHSIChannelFactory(cl);
                        FacetedSearch.CUAHSIFacetedSearch.OntologyEnvelope env = cl.GetTypedOntologyElementsGivenConstraints(SelectedFacets, BeginDateTime, EndDateTime, SrchExt.MinY, SrchExt.MaxY, SrchExt.MinX, SrchExt.MaxX, IncludeSpatialResults);
                        MyRemainingFacets = env.OntologyElements;

                        if (IncludeSpatialResults == true)
                        {
                            PutReturnOnMap(env.Sites);
                        }
                    }                    
            
                    FacetFlowPanel.Enabled = true;
                    FacetFlowPanel.Controls.Add(new SearchFacetSpecifier(this, TopLevelFacets, SelectedFacets, MyRemainingFacets, FacetFlowPanel.Controls.Count));
                    ResizeSearchSpecifiers();
                }
                catch (Exception e)
                { 
#if DEBUG
                    MessageBox.Show(string.Format("Error in transit layer with message {0} and inner exception {1}", e.Message, e.InnerException));
#else
                    MessageBox.Show("Error in transit layer. Please try again.");
#endif
                }
            }
            else
            {
                MessageBox.Show("Please select a spatial region to bound your search prior to initiating faceted search");
            }
        }

        /// <summary>
        /// Executes search for SeriesCatalog
        /// </summary>
        /// <param name="AllSelectedFacets"></param>
        public void InvokeSearchButton(List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> AllSelectedFacets)
        {
            SelectedFacets = AllSelectedFacets;
            List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> MyRemainingFacets = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
            if (InputsSufficientForFacetedSearch())
            {
                try
                {
                    SpatialTemporalCommitted = true;
                    // SrchExt = SelectedShapes.Envelope.ToExtent();
                    using (FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl = new FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient())
                    {
                        ConfigureCUAHSIChannelFactory(cl);
                        List<FacetedSearch.CUAHSIFacetedSearch.SeriesCatalogRecord> SearchRes = cl.ConductFacetedSearch(SelectedFacets, BeginDateTime, EndDateTime, SrchExt.MinY, SrchExt.MaxY, SrchExt.MinX, SrchExt.MaxX, IncludeSpatialResults).OrderByDescending(r => r.ValueCount).ToList();

                        PutReturnOnMap(SearchRes);
                        // dataGridView1.DataSource = SearchRes;
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    MessageBox.Show(string.Format("Error in transit layer with message {0} and inner exception {1}", e.Message, e.InnerException));
#else
                    MessageBox.Show("Error in transit layer. Please try again.");
#endif
                }
            }
            else
            {
                MessageBox.Show("Please select a spatial region to bound your search prior to initiating faceted search");
            }
        }

        /// <summary>
        /// Method to call next search that returns sql that would be executed, rather than resultset.
        /// </summary>
        /// <param name="AllSelectedFacets"></param>
        public void InvokeNextSQLButton(List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> AllSelectedFacets, int MyIndex)
        {
            SelectedFacets = AllSelectedFacets;
            List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> MyRemainingFacets = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
            if (InputsSufficientForFacetedSearch())
            {
                try
                {
                    RemoveDownStreamSearchFacetSpecifiers(MyIndex + 1);
                    SpatialTemporalCommitted = true;
                    // SrchExt = SelectedShapes.Envelope.ToExtent();
                    string SQLRes;
                    using (FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl = new FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient())
                    {
                        ConfigureCUAHSIChannelFactory(cl);
                        SQLRes = cl.GetSQLOfNextQuery(SelectedFacets, BeginDateTime, EndDateTime, SrchExt.MinY, SrchExt.MaxY, SrchExt.MinX, SrchExt.MaxX, IncludeSpatialResults);
                    }

                    MessageBox.Show(SQLRes);
                }
                catch (Exception e)
                {
#if DEBUG
                    MessageBox.Show(string.Format("Error in transit layer with message {0} and inner exception {1}", e.Message, e.InnerException));
#else
                    MessageBox.Show("Error in transit layer. Please try again.");
#endif
                }
            }
            else
            {
                MessageBox.Show("Please select a spatial region to bound your search prior to initiating faceted search");
            }
        }

        /// <summary>
        /// Method to call series search that returns sql that would be executed, rather than resultset.
        /// </summary>
        /// <param name="AllSelectedFacets"></param>
        public void InvokeSearchSQLButton(List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> AllSelectedFacets)
        {
            SelectedFacets = AllSelectedFacets;
            List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> MyRemainingFacets = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
            if (InputsSufficientForFacetedSearch())
            {
                try
                {
                    SpatialTemporalCommitted = true;
                    // SrchExt = SelectedShapes.Envelope.ToExtent();
                    string SQLRes;
                    using (FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient cl = new FacetedSearch.CUAHSIFacetedSearch.MultiFacetedHISSvcClient())
                    {
                        ConfigureCUAHSIChannelFactory(cl);
                        SQLRes = cl.GetSQLOfSearchQuery(SelectedFacets, BeginDateTime, EndDateTime, SrchExt.MinY, SrchExt.MaxY, SrchExt.MinX, SrchExt.MaxX, IncludeSpatialResults);
                    }

                    MessageBox.Show(SQLRes);
                }
                catch (Exception e)
                {
#if DEBUG
                    MessageBox.Show(string.Format("Error in transit layer with message {0} and inner exception {1}", e.Message, e.InnerException));
#else
                    MessageBox.Show("Error in transit layer. Please try again.");
#endif
                }

            }
            else
            {
                MessageBox.Show("Please select a spatial region to bound your search prior to initiating faceted search");
            }
        }

        /// <summary>
        /// Removes the facet that invoked the method (specified by DeleteIndex), and all facets specified after it.
        /// Triggers the re-animation of the SearchFacetSpecifier with an Index of one less (one closer to the first facet selection) than the deleted SearchFacetSpecifier
        /// </summary>
        /// <param name="DeleteIndex"></param>
        public void InvokeDeleteFacetButton(int DeleteIndex)
        {
            RemoveDownStreamSearchFacetSpecifiers(DeleteIndex);
        }

        /// <summary>
        /// Removes all SearchFacetSpecifiers after the one specified. DeleteIndex is the index of the first SearchFacetSpecifier that needs to be deleted.
        /// </summary>
        /// <param name="DeleteIndex"></param>
        private void RemoveDownStreamSearchFacetSpecifiers(int DeleteIndex)
        {
            int StartCount = FacetFlowPanel.Controls.Count;
            for (int i = DeleteIndex; i < StartCount; i++)
            {
                FacetFlowPanel.Controls.RemoveAt(DeleteIndex); // formerly RemoveAt(i)
            }
        }        

        /// <summary>
        /// Adjusts the height of the search specifiers to accomodate the new layout of the form.
        /// </summary>
        private void ResizeSearchSpecifiers()
        {
            int newHeight = SearchFacetSpecifierHeight();
            foreach (SearchFacetSpecifier s in FacetFlowPanel.Controls)
            {
                s.Height = newHeight;
            }
        }

        /// <summary>
        /// Specifies the new height of the search facet specifier objects whenever they need to be resized
        /// </summary>
        /// <returns></returns>
        private int SearchFacetSpecifierHeight()
        {
            if (FacetFlowPanel.Controls.Count > 6)
            {
                return FacetFlowPanel.Height - 20;    // 20 = height of horizontal scroll bar in px 
            }
            else
            {
                return FacetFlowPanel.Height - 5;     // 5 = decent margin
            }
        }

        /// <summary>
        /// Test method for adding blank SearchFacetSpecifiers to the FacetFlowPanel.
        /// Should deprecate upon production (demo :)).
        /// </summary>
        private void AddTestSearchFacetSpecifier()
        {
            FacetFlowPanel.Enabled = true;
            List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement> ele = new List<FacetedSearch.CUAHSIFacetedSearch.OntologyElement>();
            SearchFacetSpecifier spec = new SearchFacetSpecifier(ele, FacetFlowPanel.Controls.Count);
            spec.Height = SearchFacetSpecifierHeight();                        
            FacetFlowPanel.Controls.Add(spec);
        }
        
        /// <summary>
        /// Creates shapefile from search results, shows shapefile on map display
        /// </summary>
        /// <param name="SeriesCatalogResults"></param>
        private void PutReturnOnMap(List<FacetedSearch.CUAHSIFacetedSearch.SeriesCatalogRecord> SeriesCatalogResults)
        {
            ClearPointLayersFromMap();

            // string shapeFileName = String.Format(@"{0}\{1}.shp", Settings.Instance.TempDirectory, "FacetedSearchResult");
            FeatureSet fs = new FeatureSet(FeatureType.Point);

            fs.DataTable.Columns.Add(new DataColumn("ServCode", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("ServURL", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("SiteCode", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("VarCode", typeof(string)));
            fs.DataTable.Columns.Add(new DataColumn("StartDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("EndDate", typeof(DateTime)));
            fs.DataTable.Columns.Add(new DataColumn("ValueCount", typeof(int)));

            foreach (FacetedSearch.CUAHSIFacetedSearch.SeriesCatalogRecord o in SeriesCatalogResults)
            {
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(o.Longitude, o.Latitude);
                IFeature f = fs.AddFeature(p);
                f.DataRow.BeginEdit();                
                f.DataRow["ServCode"] = o.ServCode;
                f.DataRow["ServURL"] = o.ServURL;
                f.DataRow["SiteCode"] = o.SiteCode;
                f.DataRow["VarCode"] = o.VarCode;
                f.DataRow["StartDate"] = o.StartDate;
                f.DataRow["EndDate"] = o.EndDate;
                f.DataRow["ValueCount"] = o.ValueCount;
                f.DataRow.EndEdit();
            }

            //set the projection
            fs.Projection = 
                   new ProjectionInfo();
            fs.ProjectionString = "+proj=longlat +ellps=WGS84 +no_defs";
            
#if DEBUG            
            fs.Filename = FacetedShapeFileName;
            fs.Save();

            // implement threshold for adding to map directly or via shapefile on disk?
            if (SeriesCatalogResults.Count > 25000)
            {
            }
            else
            { 
            }

            App.Map.AddLayer(FacetedShapeFileName);

            fs.Dispose();

            //add featureSet to the map
            // IMapLayer newLayer = MapArgs.Map.Layers.Add(shapeFileName);

            //add labels
            // MapArgs.Map.AddLabels((IFeatureLayer)newLayer,
            // String.Format("[{0}]", "FacetedSearchResults"), String.Empty,
            //     new LabelSymbolizer());
#endif
        }

       
        /// <summary>
        /// Overloaded method for rendering point data on map
        /// </summary>
        /// <param name="MyRemainingFacets"></param>
        private void PutReturnOnMap(List<FacetedSearch.CUAHSIFacetedSearch.SiteData> MyRemainingFacets)
        {
            
            ClearPointLayersFromMap();
            FeatureSet fs = new FeatureSet(FeatureType.Point);
            fs.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

            foreach (FacetedSearch.CUAHSIFacetedSearch.SiteData o in MyRemainingFacets)
            {
                DotSpatial.Topology.Point p = new DotSpatial.Topology.Point();
                p.X = o.longitude;
                p.Y = o.latitude;
                fs.AddFeature(p);
            }

            fs.Filename = FacetedShapeFileName;
            fs.Save();

            App.Map.AddLayer(FacetedShapeFileName);
            // map1.Layers.Add(MyLocations);
        }

        /// <summary>
        /// Removes point layers from map
        /// </summary>
        public void ClearPointLayersFromMap()
        {
#if (DEBUG)
            // foreach (IMapLayer ptlyr in map1.GetPointLayers())
            // {
                // map1.Layers.Clear();
            //     map1.Layers.Remove(ptlyr);
            // }
            foreach (IMapLayer l in App.Map.Layers.Where(r => r.LegendText == FacetedShapeLayerName))
            {
                App.Map.Layers.Remove(l);
            }
#endif

        }
        #endregion

        #region Events
        private void FacetedSearchForm_Resize(object sender, EventArgs e)
        {
            ResizeSearchSpecifiers();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeSearchSpecifiers();
        }

        /// <summary>
        /// Resets the user interface to the starting point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, EventArgs e)
        {
            ResetInterface();
            // AddTestSearchFacetSpecifier();
        }
        
        private void InitFacetedSearchBtn_Click(object sender, EventArgs e)
        {
            InitializeFacetedSearch();
        }

        #endregion                
    }
}