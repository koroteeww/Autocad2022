using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;

namespace AutocadDLL
{
    public class AcadCommands
    {
        private int NUM = 1;
        [CommandMethod("nums")]
        public void TriggerCommand()
        {
            //to use - load DLL with netload command in autocad
            //triggers when TriggerCmd is input in console
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            try
            {

                var obj = ed.GetSelection();
                var db = Application.DocumentManager.MdiActiveDocument.Database;
                Database acCurDb = acDoc.Database;
                if (obj != null)
                {
                    var set = obj.Value;
                    var stat = obj.Status;
                    Point3d pos = new Point3d(2, 2, 0);
                    FlowDirection dir = FlowDirection.LeftToRight;
                    Double height = 0.1;
                    bool mtextSet = false;
                    MText old = new MText();
                    if (set != null)
                    {

                        foreach (SelectedObject item in set)
                        {
                            //ed.WriteMessage("selected id=" + item.ObjectId.ToString() + "   ts=" + item.ToString() + "   ");
                            //db select
                            var trans = db.TransactionManager.StartTransaction();
                            var block =
                                trans.GetObject(item.ObjectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);

                            trans.Commit();
                            var text = block.Bounds.Value;
                            if (block is DBText)
                            {
                                var txt = (DBText)block;
                                pos = txt.Position;
                                
                                mtextSet = true;
                            }
                            else if (block is MText)
                            {
                                var txt2 = (MText)block;
                                pos = txt2.Location;
                                dir = txt2.FlowDirection;
                                
                                height = txt2.Height;
                                mtextSet = true;
                                old = (MText)block;
                                //ed.WriteMessage("block=" + block.ToString() + "   dir=" + txt2.Direction+" rot="+ txt2.Rotation+" w="+txt2.Width+" h="+txt2.Height );
                            }
                            else
                            {
                                ed.WriteMessage("block type =" + block.GetType() + " \r\n");
                            }
                            
                        }

                        //CREATE TEXT
                        // Start a transaction
                        using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                        {
                            // Open the Block table for read
                            BlockTable acBlkTbl;
                            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                               OpenMode.ForRead) as BlockTable;
                            // Open the Block table record Model space for write
                            BlockTableRecord acBlkTblRec;
                            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                                  OpenMode.ForWrite) as BlockTableRecord;
                            // Create a single-line text object
                            //MText acText = new MText();

                            //acText.SetDatabaseDefaults();

                            //clone from old text
                            MText acText = (MText)old.Clone();
                            //POSITION
                            if (mtextSet)
                            {
                                double shift = 0.05;

                                if ( Math.Abs(acText.Direction.Y - 1.0) < 0.02)
                                {
                                    //text is vertical
                                    //shift pos 0.5 to down
                                    Vector3d v = new Vector3d(0, shift, 0);
                                    Point3d posn = pos.Subtract(v);
                                    acText.Location = posn;
                                }
                                else if (Math.Abs(acText.Direction.X - 1.0) < 0.02)
                                {
                                    //text is horizontal
                                    //shift pos 0.5 to left
                                    Vector3d v = new Vector3d(shift, 0, 0);
                                    Point3d posn = pos.Subtract(v);
                                    acText.Location = posn;

                                }
                                else 
                                {
                                    //more math... text can be 4 ways oriented
                                    if (acText.Direction.X > 0.02 && acText.Direction.Y > 0.02)
                                    {
                                        //shift pos 0.5 to both
                                        Vector3d v = new Vector3d(shift, shift, 0);
                                        Point3d posn = pos.Subtract(v);
                                        acText.Location = posn;
                                    }
                                    else if (acText.Direction.X > 0.02 && acText.Direction.Y < 0.02)
                                    {
                                        Vector3d v = new Vector3d(shift, -shift, 0);
                                        Point3d posn = pos.Subtract(v);
                                        acText.Location = posn;
                                    }
                                    else if (acText.Direction.X < 0.02 && acText.Direction.Y < 0.02)
                                    {
                                        Vector3d v = new Vector3d(-shift, -shift, 0);
                                        Point3d posn = pos.Subtract(v);
                                        acText.Location = posn;
                                    }
                                    else if (acText.Direction.X < 0.02 && acText.Direction.Y > 0.02)
                                    {
                                        Vector3d v = new Vector3d(-shift, shift, 0);
                                        Point3d posn = pos.Subtract(v);
                                        acText.Location = posn;
                                    }
                                    
                                }

                                //acText.FlowDirection = dir;
                                //acText.Height = 0.2;
                                //acText.Height = height;

                                acText.Contents = "" + NUM.ToString();
                                acText.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);
                                acBlkTblRec.AppendEntity(acText);
                                acTrans.AddNewlyCreatedDBObject(acText, true);
                                //CIRCLE
                                //
                                Circle acCirc = new Circle();
                                acCirc.SetDatabaseDefaults();
                                var minp = acText.Bounds.Value.MinPoint;
                                var maxp=  acText.Bounds.Value.MaxPoint;
                                //y+h/2 x+w/2
                                Point3d BigRectCenter = new Point3d((acText.ActualWidth / 2) + acText.Location.X, 
                                    (acText.ActualHeight / 2) + acText.Location.Y, 0);
                                acCirc.Center = BigRectCenter;
                                acCirc.Diameter = acText.ActualHeight + 0.1;
                                //debug
                                ed.WriteMessage("CIRCLE Center=" + BigRectCenter + " diam=" + acCirc.Diameter
                                    + " test Height=" + acText.Height+ " test ActualHeight=" + acText.ActualHeight
                                    + " bounds=" + minp+ " "+maxp);
                                

                                acCirc.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);

                                // Add the new object to the block table record and the transaction
                                acBlkTblRec.AppendEntity(acCirc);
                                acTrans.AddNewlyCreatedDBObject(acCirc, true);

                                // Save the changes and dispose of the transaction
                                acTrans.Commit();
                                ed.WriteMessage("\r\n text OK at " + acText.Location);
                            }
                            else
                            {

                                //acText.Position = new Point3d(2, 2, 0);
                            }
                           
                        }
                    }
                    else
                    {
                        ed.WriteMessage("set null");
                    }
                }
                else
                {
                    ed.WriteMessage("select null");
                }
                Point2d p;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage(ex.Message);
            }
            //ed.WriteMessage("Hi C# " + NUM.ToString());
            NUM++;
        }
    }
}
