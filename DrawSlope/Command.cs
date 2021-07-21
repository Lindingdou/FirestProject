using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Windows;
using System.Windows.Forms;


namespace DrawSlope
{
    public class Command
    {

        [CommandMethod("test")]
        public void Test()
        {


            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Point3d pta = new Point3d(1, 3, 0);
            Point3d ptC = new Point3d(4, 6, 0);
            Base bs = new Base();
            Point3dCollection pts = bs.GetPoint3dC(pta, ptC, 1, false);
            Polyline3d pol3d = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            bs.SaveDB(db, pol3d);



        }


        [CommandMethod("droad")]
        public void DrawRoad()
        {

            //Test
            double width;
            bool dir;
            double slope;
            bool isCon = true;


            //给这三个参数赋值
            Param paraWin = new Param();
            paraWin.ShowDialog();

            if (!paraWin.isCon)
            {

                return;
            }
            slope = paraWin.slope;
            dir = paraWin.isDir;
            width = paraWin.width;
            Database db = HostApplicationServices.WorkingDatabase;

            Base bs = new Base();




            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            //拾取A点和求C点

            PromptEntityResult ptr = ed.GetEntity("请选择坡底线");
            if (ptr.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\n坡底线选择失败，请重新尝试此功能");
                return;
                //ed.WriteMessage("ok选择成");
            }
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Polyline3d poly3d_down = trans.GetObject(ptr.ObjectId, OpenMode.ForRead) as Polyline3d;
                if (poly3d_down == null)
                {
                    ed.WriteMessage("\n请将坡底线转换为三维多段线后尝试此功能");
                    return;
                }
                //尝试选择第二条线
                PromptEntityResult ptr2 = ed.GetEntity("\n请选择坡顶线");
                if (ptr2.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("\n坡顶线选择失败，请重新尝试此功能");
                    return;
                }
                Polyline3d poly3d_up = trans.GetObject(ptr2.ObjectId, OpenMode.ForRead) as Polyline3d;
                if (poly3d_up == null)
                {
                    ed.WriteMessage("\n请将坡顶线转换为三维多段线后尝试此功能");
                    return;
                }
                Point3dCollection snapPts1 = new Point3dCollection();
                IntegerCollection geomIds1 = new IntegerCollection(1);
                geomIds1.Add(0);
                //获取最近点
                poly3d_down.GetObjectSnapPoints(ObjectSnapModes.ModeNear, 0, ptr.PickedPoint, ptr.PickedPoint, Matrix3d.Identity, snapPts1, geomIds1);
                Point3dCollection snapPts2 = new Point3dCollection();
                IntegerCollection geomIds2 = new IntegerCollection(1);
                geomIds2.Add(0);
                //获取最近点
                poly3d_up.GetObjectSnapPoints(ObjectSnapModes.ModeNear, 0, ptr2.PickedPoint, ptr2.PickedPoint, Matrix3d.Identity, snapPts2, geomIds2);




                poly3d_up.UpgradeOpen();
                poly3d_down.UpgradeOpen();
                if (snapPts2.Count == 0 || snapPts1.Count == 0)
                {
                    ed.WriteMessage("\n无法计算出两线高程，请重新尝试此功能！");
                }
                //计算两条线高差
                Point3d pt_up = bs.GetPt3d(poly3d_up, ptr.PickedPoint);
                //开始指定插入点
                Point3d pt_down = bs.GetPt3d(poly3d_down, ptr2.PickedPoint);
                double height = pt_up.Z - pt_down.Z;



                if (height <= 0.0001)
                {

                    ed.WriteMessage("\n两线间不存在高差，无需绘制斜坡道");
                    return;

                }


                //坡道参数
                //double width = 4;
                double len = height / slope;

                //计算C点
                Circle c_Cal = new Circle(new Point3d(ptr.PickedPoint.X, ptr.PickedPoint.Y, 0), new Vector3d(0, 0, 1), len);
                //确定插入点

                //求交点

                //画坡道


                Point3dCollection pts_Inter = new Point3dCollection();
                Point3dCollection temPts = new Point3dCollection();
                ////求二维圆和三维多段线的交点
                //Polyline3d tem_Pl = bs.Pl3t2(poly3d_up);
                //Polyline3d tem_Pl_D = bs.Pl3t2(poly3d_down);

                SlopeJig jig = new SlopeJig(new Point3d(ptr.PickedPoint.X, ptr.PickedPoint.Y, 0), poly3d_down, poly3d_up, c_Cal, dir, width);
                PromptResult resJig = ed.Drag(jig);

                if (resJig.Status == PromptStatus.OK)
                {

                    List<Polyline3d> objlist = bs.Rec(poly3d_down.GetClosestPointTo(jig.basePt, false), poly3d_up.GetClosestPointTo(jig.pt2_out, false), width, dir);
                    //求交点
                    //bs.SaveDB(db, objlist[0]);
                    //bs.SaveDB(db, objlist[1]);
                    //bs.SaveDB(db, objlist[2]);
                    Point3dCollection inter_pts_1 = new Point3dCollection();//objlist[1]与坡顶线相交
                    objlist[1].IntersectWith(poly3d_up, Intersect.OnBothOperands, new Plane(Point3d.Origin, Vector3d.ZAxis), inter_pts_1, 0, 0);


                    Point3dCollection inter_pts_2 = new Point3dCollection();
                    objlist[2].IntersectWith(poly3d_down, Intersect.OnBothOperands, new Plane(Point3d.Origin, Vector3d.ZAxis), inter_pts_2, 0, 0);


                    //Point3d p1 = poly3d_up.GetClosestPointTo(inter_pts_1[0], false); //
                    //Point3d p2 = poly3d_down.GetClosestPointTo(inter_pts_2[0], false);//



                    Point3dCollection pts = new Point3dCollection();
                    objlist[0].GetStretchPoints(pts);

                    Point3d pside1_1 = pts[3];//poly3d_up.GetClosestPointTo(inter_pts_1[0], false);
                    Point3d pside1_2 = poly3d_up.GetClosestPointTo(inter_pts_1[0], false);//bs.GetPt3d(poly3d_up, inter_pts_2[0]);
                    //Polyline3d polside1 = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection { pside1_1, poly3d_up.GetClosestPointTo(jig.pt2_out, false), pside1_2 }, false);
                    objlist.RemoveAt(2);
                    //objlist.Add(polside1);
                    Point3d pside2_1 = pts[0];//poly3d_down.GetClosestPointTo(inter_pts_2[0], false);
                    Point3d pside2_2 = bs.GetPt3d(poly3d_down, inter_pts_2[0]);
                    //Polyline3d polside2 = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection { pside2_1, poly3d_down.GetClosestPointTo(jig.basePt, false), pside2_2 }, false);
                    objlist.RemoveAt(1);
                    //objlist.Add(polside2);


                    //重新画线

                    Point3dCollection pt_Up = new Point3dCollection();
                    Point3dCollection pt_Down = new Point3dCollection();

                    poly3d_up.GetStretchPoints(pt_Up);
                    poly3d_down.GetStretchPoints(pt_Down);



                    int index_pts2 = bs.Sindex(pt_Up, pts[2]);
                    int index_side1_2 = bs.Sindex(pt_Up, pside1_2);

                    int index_pts0 = bs.Sindex(pt_Down, pts[0]);
                    int index_side2_2 = bs.Sindex(pt_Down, pside2_2);



                    if (index_pts2 > index_side1_2)
                    {


                        for (int i = 1; i <= index_pts2 - index_side1_2; i++)
                        {
                            pt_Up.RemoveAt(index_side1_2);

                        }

                        if (dir)
                        {
                            //应该进这个位置
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[2]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[3]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pside1_2));

                        }
                        else
                        {
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[2]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[3]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pside1_2));


                        }




                    }
                    else if (index_pts2 == index_side1_2)
                    {
                        if (dir)
                        {
                            //应该进这个位置
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[2]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[3]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pside1_2));

                        }
                        else
                        {
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pside1_2));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[3]));
                            pt_Up.Insert(index_side1_2, bs.GetPt3d(poly3d_up, pts[2]));

                        }
                    }
                    else
                    {
                        for (int i = 1; i <= -index_pts2 + index_side1_2; i++)
                        {
                            pt_Up.RemoveAt(index_pts2);
                        }
                        if (dir)
                        {
                            pt_Up.Insert(index_pts2, bs.GetPt3d(poly3d_up, pside1_2));
                            pt_Up.Insert(index_pts2, bs.GetPt3d(poly3d_up, pts[3]));
                            pt_Up.Insert(index_pts2, bs.GetPt3d(poly3d_up, pts[2]));


                        }
                        else
                        {


                            pt_Up.Insert(index_pts2, bs.GetPt3d(poly3d_up, pside1_2));
                            pt_Up.Insert(index_pts2, bs.GetPt3d(poly3d_up, pts[3]));
                            pt_Up.Insert(index_pts2, bs.GetPt3d(poly3d_up, pts[2]));

                        }





                    }


                    if (index_pts0 > index_side2_2)
                    {
                        for (int i = 1; i <= index_pts0 - index_side2_2; i++)
                        {
                            pt_Down.RemoveAt(index_side2_2);

                        }

                        if (dir)
                        {
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[0]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[1]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pside2_2));


                        }
                        else
                        {
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[0]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[1]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pside2_2));

                        }


                    }
                    else if (index_pts0 == index_side2_2)
                    {
                        if (dir)
                        {
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pside2_2));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[1]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[0]));


                        }
                        else
                        {


                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[0]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pts[1]));
                            pt_Down.Insert(index_side2_2, bs.GetPt3d(poly3d_down, pside2_2));

                        }
                    }
                    else
                    {
                        for (int i = 1; i <= -index_pts0 + index_side2_2; i++)
                        {
                            pt_Down.RemoveAt(index_pts0);
                        }
                        if (dir)
                        {
                            pt_Down.Insert(index_pts0, bs.GetPt3d(poly3d_down, pside2_2));
                            pt_Down.Insert(index_pts0, bs.GetPt3d(poly3d_down, pts[1]));
                            pt_Down.Insert(index_pts0, bs.GetPt3d(poly3d_down, pts[0]));






                        }
                        else
                        {
                            pt_Down.Insert(index_pts0, bs.GetPt3d(poly3d_down, pside2_2));
                            pt_Down.Insert(index_pts0, bs.GetPt3d(poly3d_down, pts[1]));
                            pt_Down.Insert(index_pts0, bs.GetPt3d(poly3d_down, pts[0]));


                        }

                    }



                    //在坡底找pts[0]和side2_2的索引

                    poly3d_up.UpgradeOpen();

                    poly3d_up.Erase();
                    Polyline3d poly3dNew = new Polyline3d(Poly3dType.SimplePoly, pt_Up, false);
                    poly3dNew.Color = poly3d_up.Color;

                    objlist.Add(poly3dNew);

                    poly3d_down.UpgradeOpen();
                    Polyline3d pol_d = new Polyline3d(Poly3dType.SimplePoly, pt_Down, false);
                    pol_d.Color = poly3d_down.Color;
                    poly3d_down.Erase();

                    objlist.Add(pol_d);



                    Point3d aMb = new Point3d((pts[0].X + pts[1].X) / 2, (pts[0].Y + pts[1].Y) / 2, (pts[0].Z + pts[1].Z) / 2);
                    Point3d cMd = new Point3d((pts[2].X + pts[3].X) / 2, (pts[2].Y + pts[3].Y) / 2, (pts[2].Z + pts[3].Z) / 2);
                    Polyline3d polyMid = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection() { aMb, cMd }, false);
                    
                    
                    

                    objlist.Add(polyMid);

                    Point3d mj1ab = new Point3d(aMb.X + 3, aMb.Y + 3 + Math.Sqrt(3), aMb.Z);
                    Point3d mj2ab = new Point3d(aMb.X - 3, aMb.Y + 3 + Math.Sqrt(3), aMb.Z);
                    Polyline3d polymjab = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection() { aMb,mj1ab, mj2ab }, true);
                    polymjab.ColorIndex = 2;
                    objlist.Add(polymjab);

                    

                    Point3d mj1cd = new Point3d(cMd.X + 3, cMd.Y + 3 + Math.Sqrt(3), cMd.Z);
                    Point3d mj2cd = new Point3d(cMd.X - 3, cMd.Y + 3 + Math.Sqrt(3), cMd.Z);
                    Polyline3d polymjcd = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection() { cMd,mj1cd, mj2cd }, true);
                    polymjcd.ColorIndex = 2;
                    objlist.Add(polymjcd);
                   



                    DBText t1 = new DBText();
                    t1.Position = aMb;
                    t1.VerticalMode = TextVerticalMode.TextTop;
                    t1.HorizontalMode = TextHorizontalMode.TextCenter;
                    t1.ColorIndex = 2;
                    t1.TextString = aMb.Z.ToString("0.00");

                    t1.AlignmentPoint = aMb;
                    t1.AdjustAlignment(HostApplicationServices.WorkingDatabase);
                    bs.SaveDB(db, t1);

                    DBText t2 = new DBText();
                    t2.Position = cMd;
                    t2.VerticalMode = TextVerticalMode.TextTop;
                    t2.HorizontalMode = TextHorizontalMode.TextCenter;
                    t2.ColorIndex = 2;
                    t2.TextString = cMd.Z.ToString("0.00");
                    t2.AlignmentPoint = cMd;
                    t2.AdjustAlignment(HostApplicationServices.WorkingDatabase);
                    bs.SaveDB(db, t2);


                    Point3d abMcd = new Point3d((aMb.X + cMd.X) / 2, (aMb.Y + cMd.Y) / 2, (aMb.Z + cMd.Z) / 2);
                    DBText t3 = new DBText();
                    t3.Position = abMcd;
                    t3.VerticalMode = TextVerticalMode.TextBottom;
                    t3.HorizontalMode = TextHorizontalMode.TextCenter;
                    t3.ColorIndex = 2;
                    t3.TextString = (slope*100).ToString()+"°";


                    t3.AlignmentPoint = abMcd;
                    t3.AdjustAlignment(HostApplicationServices.WorkingDatabase);                  
                    
                   
                    bs.SaveDB(db, t3);



                    DBText t4 = new DBText();
                    t4.Position = abMcd;
                    t4.VerticalMode = TextVerticalMode.TextTop;
                    t4.HorizontalMode = TextHorizontalMode.TextCenter;
                    t4.ColorIndex = 2;
                    t4.TextString = polyMid.Length.ToString("0.00");


                    t4.AlignmentPoint = abMcd;
                    t4.AdjustAlignment(HostApplicationServices.WorkingDatabase);                    
                    
                    
                    bs.SaveDB(db, t4);

                    objlist.ForEach((a) =>
                    {

                        bs.SaveDB(db, a);
                    });









                }






                trans.Commit();
            }



        }


    }
}
