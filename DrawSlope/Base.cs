using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Windows;

namespace DrawSlope
{
    class Base
    {

        private const double PI = 3.1415926;
        /// <summary>
        /// 绘制坡道图形
        /// </summary>
        /// <param name="sPt1"></param>
        /// <param name="sPt2"></param>
        /// <param name="direct"></param>
        /// <param name="width"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public List<Polyline3d> Rec(Point3d sPt1, Point3d sPt2, double width, bool isDir)
        {
            Point2d A = new Point2d(sPt1.X, sPt1.Y);
            Point2d C = new Point2d(sPt2.X, sPt2.Y);

            //Point2d D = Cal_Insert_Point(A, C, width, isDir);

            //Point2d B = new Point2d((A.X + C.X) - D.X, (A.Y + C.Y) - D.Y);

            //Point3dCollection pts = new Point3dCollection();
            //pts.Add(sPt1);//A
            //pts.Add(new Point3d(B.X, B.Y, sPt2.Z)); //B
            //pts.Add(sPt2);//C
            //pts.Add(new Point3d(D.X, D.Y, sPt1.Z));//D
            Point3dCollection pts = GetPoint3dC(sPt1, sPt2, width, isDir);
            Point2d B = new Point2d(pts[1].X, pts[1].Y);
            Point2d D = new Point2d(pts[3].X, pts[3].Y);

            //顺时针需要加-；
            //逆时针需要加+
            Point2d roC;
            Point2d roA;
            if (isDir)
            {  //将C点绕D点旋转
                roC = PtRoate(B, C, -2* PI / 180);
                //将A点绕B旋转
                roA = PtRoate(D, A, -2 * PI / 180);

            }
            else
            {
                //将C点绕D点旋转
                roC = PtRoate(B, C, 2 * PI / 180);
                //将A点绕B旋转
                roA = PtRoate(D, A, 2 * PI / 180);
            }



            Polyline3d poly3d_side_DC = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection { new Point3d(D.X, D.Y, sPt2.Z), new Point3d(roA.X, roA.Y, sPt2.Z) }, false);
            Polyline3d poly3d_side_BA = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection { new Point3d(B.X, B.Y, sPt1.Z), new Point3d(roC.X, roC.Y, sPt1.Z) }, false);



            List<Polyline3d> poly3d_List = new List<Polyline3d>();
            poly3d_List.Add(new Polyline3d(Poly3dType.SimplePoly, pts, true));
            poly3d_List.Add(poly3d_side_DC);
            poly3d_List.Add(poly3d_side_BA);


            return poly3d_List;
        }
        /// <summary>
        /// 计算多段线上的临近点
        /// </summary>
        /// <param name="poly3d"></param>
        /// <param name="pt2d"></param>
        /// <returns></returns>
        public Point3d GetPt3d(Polyline3d poly3d, Point3d pt)
        {
            Point3dCollection pts = new Point3dCollection();
            poly3d.GetStretchPoints(pts);
            double Dis1 = 100000000;
            double Dis2 = 100000000;
            int index1 = 0;
            int index2 = 0;

            if (pts.Count != 0)
            {

                for (int i = 0; i < pts.Count; i++)
                {
                    double tem_Dis = new Point2d(pt.X, pt.Y).GetDistanceTo(new Point2d(pts[i].X, pts[i].Y));

                    if (tem_Dis < Dis1)
                    {
                        //tem比他俩都小
                        if (tem_Dis < Dis1)
                        {
                            Dis1 = tem_Dis;
                            index1 = i;
                        }

                    }

                }

            }
            int index22 = index1 + 1;

            int index3 = index1 - 1;





            if (index22 >= pts.Count || index3 < 0)
            {
                return Point3d.Origin;
            }
            double tem_Dis2 = new Point2d(pts[index22].X, pts[index22].Y).GetDistanceTo(new Point2d(pt.X, pt.Y));
            double tem_Dis3 = new Point2d(pts[index3].X, pts[index3].Y).GetDistanceTo(new Point2d(pt.X, pt.Y));

            if (tem_Dis2 < tem_Dis3)
            {
                Dis2 = tem_Dis2;
                index2 = index22;
            }
            else
            {
                Dis2 = tem_Dis3;
                index2 = index3;

            }


            double z = (Dis1 / (Dis1 + Dis2)) * (pts[index2].Z - pts[index1].Z) + pts[index1].Z;

            //DBPoint p1 = new DBPoint(pts[index1]);
            //DBPoint p2 = new DBPoint(pts[index2]);
            //DBPoint p3 = new DBPoint(new Point3d(pt.X, pt.Y, z));
            //p3.ColorIndex = 8;
            //Database db = Application.DocumentManager.MdiActiveDocument.Database;
            //Base bs = new Base();
            //bs.SaveDB(db, p1);
            //bs.SaveDB(db, p2);
            //bs.SaveDB(db, p3);


            return new Point3d(pt.X, pt.Y, z);

        }
        public int Sindex(Point3dCollection pts, Point3d pt)
        {
            //double Dis1 = 100000000;
            //int index1 = 0;
            //if (pts.Count != 0)
            //{
            //    for (int i = 0; i < pts.Count; i++)
            //    {
            //        double tem_Dis = new Point2d(pt.X, pt.Y).GetDistanceTo(new Point2d(pts[i].X, pts[i].Y));
            //        if (tem_Dis < Dis1)
            //        {
            //            //tem比他俩都小
            //            Dis1 = tem_Dis;
            //            index1 = i;
            //        }

            //    }

            //}
            //return index1;
            int index = 0;
            Point3d ptFirst = pts[0];
            for (int i = 1; i < pts.Count; i++)
            {
                if (onsegment(ptFirst,pts[i],pt))
                {
                    index = i;
                }
                ptFirst = pts[i];
            }
            return index;

        }

        bool onsegment(Point3d pi, Point3d pj, Point3d Q)
        {
            if (Math.Abs((Q.X - pi.X) * (pj.Y - pi.Y) - (pj.X - pi.X) * (Q.Y - pi.Y)) <0.001&& Math.Min(pi.X, pj.X) <= Q.X && Q.X <= Math.Max(pi.X, pj.X) && Math.Min(pi.Y, pj.Y) <= Q.Y && Q.Y <= Math.Max(pi.Y, pj.Y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Polyline3d Pl3t2(Polyline3d poly3d)
        {
            Point3dCollection pts = new Point3dCollection();
            Point3dCollection pts3d = new Point3dCollection();
            poly3d.GetStretchPoints(pts);
            foreach (Point3d pt in pts)
            {
                pts3d.Add(new Point3d(pt.X, pt.Y, 0));

            }
            Polyline3d poly = new Polyline3d(Poly3dType.SimplePoly, pts3d, false);
            return poly;
            //return new Point3d(0, 0, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        Point2d PtRoate(Point2d pt0, Point2d pt, double angle)
        {
            double x = (pt.X - pt0.X) * Math.Cos(angle) - (pt.Y - pt0.Y) * Math.Sin(angle) + pt0.X;
            double y = (pt.X - pt0.X) * Math.Sin(angle) + (pt.Y - pt0.Y) * Math.Cos(angle) + pt0.Y;
            return new Point2d(x, y);
        }
        /// <summary>
        /// 已知两点求长方形角点
        /// </summary>
        /// <param name="A"></param>
        /// <param name="C"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        Point2d Cal_Insert_Point(Point2d A, Point2d C, double width, bool dir)
        {
            double r1 = width, r2 = width;
            double rc = A.GetDistanceTo(C) / 2;//A、C两点距离的一半
            double x1 = A.X;
            double x2 = C.X;
            double y1 = A.Y;
            double y2 = C.Y;
            double xc = (x1 + x2) / 2;//AC中点的X、Y坐标
            double yc = (y1 + y2) / 2;

            //构造关系式系数
            double a1 = -(x1 - xc) / (y1 - yc);
            double a2 = (x2 - xc) / (y2 - yc);
            double b1 = ((rc - r1) * (rc + r1) + (y1 - yc) * (y1 + yc) + x1 * x1 - xc * xc) / (2 * (y1 - yc));
            double b2 = ((rc - r2) * (rc + r2) + (y2 - yc) * (y2 + yc) + x2 * x2 - xc * xc) / (2 * (y2 - yc));

            double delt1 = Math.Pow(2 * a1 * (b1 - y1) - 2 * x1, 2) - 4 * (1 + a1 * a1) * (Math.Pow(x1, 2) + Math.Pow(b1 - y1, 2) - r1 * r1);
            double delt2 = Math.Pow(2 * a2 * (b2 - y2) - 2 * x2, 2) - 4 * (1 + a2 * a2) * (Math.Pow(x2, 2) + Math.Pow(b2 - y2, 2) - r2 * r2);
            //两个交点
            double inter_x_up = (-(2 * a1 * (b1 - y1) - 2 * x1) + Math.Sqrt(delt1)) / (2 * (1 + a1 * a1));
            double inter_x_down = (-(2 * a1 * (b1 - y1) - 2 * x1) - Math.Sqrt(delt1)) / (2 * (1 + a1 * a1));

            double inter_y_up = b1 + a1 * inter_x_up;
            double inter_y_down = b1 + a1 * inter_x_down;

            //此处采用这种判断有局限性
            Vector2d vec2d_1 = new Vector2d(inter_x_up - x1, inter_y_up - y1);
            Vector2d vec2d_2 = new Vector2d(inter_x_down - x1, inter_y_down - y1);

            Vector2d vec = new Vector2d(x2 - x1, y2 - y1);

            if (dir)//顺时针
            {
                Point2d p1 = new Point2d(inter_x_up, inter_y_up);
                Point2d p2 = new Point2d(inter_x_down, inter_y_down);
                double angle = Math.Abs(vec.GetAngleTo(vec2d_1));
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                //ed.WriteMessage(angle.ToString());
                if (angle < PI / 4)
                {
                    return p1;
                }
                else
                {
                    return p2;
                }


            }
            else
            {

                Point2d p1 = new Point2d(inter_x_up, inter_y_up);
                Point2d p2 = new Point2d(inter_x_down, inter_y_down);
                double angle = Math.Abs(vec.GetAngleTo(vec2d_1));
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                //ed.WriteMessage(angle.ToString());
                if (angle < PI / 4)
                {
                    return p1;
                }
                else
                {
                    return p2;
                }

            }


        }
        /// <summary>
        /// 保存实体的方法
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        public ObjectId SaveDB(Database db, Entity ent)
        {

            //Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                ObjectId entId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
                return entId;
            }

        }

        public Point3dCollection GetPoint3dC(Point3d A, Point3d C, double width, bool dir)
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Point3dCollection pts = new Point3dCollection();
            double Midx = (A.X + C.X) / 2;
            double Midy = (A.Y + C.Y) / 2;
            //辅助计算圆
            Circle circle = new Circle(new Point3d(Midx, Midy, 0), Vector3d.ZAxis, A.DistanceTo(C) / 2);
            Vector2d AC = new Vector2d(C.X - A.X, C.Y - A.Y);
            Vector2d CA = new Vector2d(A.X - C.X, A.Y - C.Y);
            double ac2x = AC.GetAngleTo(Vector2d.XAxis);
            double ca2x = CA.GetAngleTo(Vector2d.XAxis);

            Circle cirA = new Circle(new Point3d(A.X, A.Y, 0), Vector3d.ZAxis, width);
            Circle cirC = new Circle(new Point3d(C.X, C.Y, 0), Vector3d.ZAxis, width);



            Point3dCollection ptsB = new Point3dCollection();
            Point3dCollection ptsD = new Point3dCollection();
            circle.IntersectWith(cirA, Intersect.OnBothOperands, ptsB, 0, 0);
            circle.IntersectWith(cirC, Intersect.OnBothOperands, ptsD, 0, 0);


            double inferB = (C.X - A.X) * (ptsB[0].Y - A.Y) - (C.Y - A.Y) * (ptsB[0].X - A.X);
            pts.Add(A);
            if (inferB < 0)//ptsB[0]的点在AC右侧
            {
                if (dir)
                {
                    pts.Add(new Point3d(ptsB[0].X, ptsB[0].Y, A.Z));
                }
                else
                {
                    pts.Add(new Point3d(ptsB[1].X, ptsB[1].Y, A.Z));
                }
            }
            else//说明ptsB[0]的点在AC左侧
            {
                if (dir)
                {
                    pts.Add(new Point3d(ptsB[1].X, ptsB[1].Y, A.Z));
                }
                else
                {
                    pts.Add(new Point3d(ptsB[0].X, ptsB[0].Y, A.Z));
                }
            }
            pts.Add(C);
            double inferD = (A.X - C.X) * (ptsD[0].Y - C.Y) - (A.Y - C.Y) * (ptsD[0].X - C.X);
            if (inferD < 0)//ptsD[0]的点在AC右侧
            {
                if (dir)
                {
                    pts.Add(new Point3d(ptsD[0].X, ptsD[0].Y, C.Z));
                }
                else
                {
                    pts.Add(new Point3d(ptsD[1].X, ptsD[1].Y, C.Z));
                }
            }
            else//说明ptsB[0]的点在AC左侧
            {
                if (dir)
                {
                    pts.Add(new Point3d(ptsD[1].X, ptsD[1].Y, C.Z));
                }
                else
                {
                    pts.Add(new Point3d(ptsD[0].X, ptsD[0].Y, C.Z));
                }
            }

            //if (dir)//顺时针
            //{


            //    //bs.SaveDB(db, arcA);
            //    //bs.SaveDB(db, arcB);


            //    pts.Add(new Point3d(ptsB[0].X, ptsB[0].Y, A.Z));
            //    pts.Add(C);
            //    pts.Add(new Point3d(ptsD[0].X, ptsD[0].Y, C.Z));
            //}
            //else
            //{
            //    Arc arcA = new Arc(new Point3d(A.X, A.Y, 0), width, ac2x, ac2x - Math.PI);
            //    Arc arcB = new Arc(new Point3d(C.X, C.Y, 0), width, -ca2x, -ca2x + Math.PI);
            //    //bs.SaveDB(db, arcA);
            //    //bs.SaveDB(db, arcB);
            //    circle.IntersectWith(arcA, Intersect.OnBothOperands, new Plane(Point3d.Origin, Vector3d.ZAxis), ptsB, 0, 0);
            //    circle.IntersectWith(arcB, Intersect.OnBothOperands, new Plane(Point3d.Origin, Vector3d.ZAxis), ptsD, 0, 0);
            //    pts.Add(A);
            //    pts.Add(new Point3d(ptsB[0].X, ptsB[0].Y, A.Z));
            //    pts.Add(C);
            //    pts.Add(new Point3d(ptsD[0].X, ptsD[0].Y, C.Z));
            //}






            return pts;

        }
    }
}
