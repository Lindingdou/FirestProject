using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Colors;
namespace DrawSlope
{
    class SlopeJig : DrawJig
    {
        private Circle cir;
        private Polyline3d polyUp;
        List<Polyline3d> recs;
        private Point3dCollection pts;
        public Point3d basePt;
        public Point3d pt2_out;
        private bool dir;
        private Polyline3d polyDown;
        private double width;
        private Arc arc_fz;
        

        public SlopeJig(Point3d basePoint, Polyline3d polyD, Polyline3d polyU, Circle circle, bool direct, double w)
        {
            //将图形传入
            cir = circle;
            polyUp = polyU;
            polyDown = polyD;
            pts = new Point3dCollection();
            dir = direct;
            basePt = basePoint;
            width = w;
            
        }
        protected override bool WorldDraw(WorldDraw draw)
        {
            draw.Geometry.Draw(cir);
            draw.Geometry.Draw(recs[0]);
            draw.Geometry.Draw(recs[1]);
            draw.Geometry.Draw(recs[2]);

            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            JigPromptPointOptions optJigPoint = new JigPromptPointOptions("\n 请指定坡道插入位置");
            optJigPoint.Cursor = CursorType.Crosshair;
            optJigPoint.BasePoint = basePt.TransformBy(mt);
            optJigPoint.UseBasePoint = true;

            PromptPointResult resJig = prompts.AcquirePoint(optJigPoint);
            Point3d temPt = resJig.Value;
            if (basePt != polyDown.GetClosestPointTo(temPt, false))
            {
                pts.Clear();
                
                cir.Center = polyDown.GetClosestPointTo(temPt, false);
                cir.IntersectWith(polyUp, Intersect.OnBothOperands,new Plane(Point3d.Origin,Vector3d.ZAxis), pts, 0, 0);
                //判断这个点在那一侧
                if (pts.Count == 2)
                {
                    Point2d pt2d1 = new Point2d(pts[0].X, pts[0].Y);
                    Point2d pt2d2 = new Point2d(pts[1].X, pts[1].Y);
                    Point2d P1 = new Point2d(polyDown.GetClosestPointTo(temPt, false).X, polyDown.GetClosestPointTo(temPt, false).Y);//p1
                    Point2d P2 = new Point2d((pt2d1.X + pt2d2.X) / 2, (pt2d1.Y + pt2d2.Y) / 2);//中点
                    double v1 = (P2.X - P1.X) * (pt2d2.Y - P1.Y) - (P2.Y - P1.Y) * (pt2d2.X - P1.X);
                    
                    Point3d pt2;
                    if (v1 < 0)//pt2d2为右侧点
                    {
                        //pts[1]为右侧的点
                        if (dir)//顺时针
                        {
                            pt2 = pts[1];

                        }
                        else
                        {
                            pt2 = pts[0];
                        }
                    }
                    else//pt2d2为左侧
                    {
                        //v2<0
                        //pts[0]为右侧的点
                        if (dir)
                        {
                            pt2 = pts[0];
                        }
                        else
                        {
                            pt2 = pts[1];
                        }
                    }
                    if (pt2 != null)
                    {
                        Base bs = new Base();
                        recs = bs.Rec(polyDown.GetClosestPointTo(temPt, false), polyUp.GetClosestPointTo(pt2,false), width, dir);

                        recs[0].TransformBy(Matrix3d.Displacement(basePt.GetVectorTo(polyDown.GetClosestPointTo(temPt, false))));
                        recs[1].TransformBy(Matrix3d.Displacement(basePt.GetVectorTo(polyDown.GetClosestPointTo(temPt, false))));
                        recs[2].TransformBy(Matrix3d.Displacement(basePt.GetVectorTo(polyDown.GetClosestPointTo(temPt, false))));

                        pt2_out = polyUp.GetClosestPointTo(pt2, false);
                    }

                }


                basePt = polyDown.GetClosestPointTo(temPt, false);

                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }

        }
    }
}
