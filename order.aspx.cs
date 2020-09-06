using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using eCommerceDBModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
public partial class order : System.Web.UI.Page
{


    protected void Page_Load(object sender, EventArgs e)
    {
       
       
        //---------------------------------------------------
        
        if (Session["CustomerID"] == null)
        {
            string redirectToURL = "~/order.aspx";
            Response.Redirect("~/login.aspx?redirect=" + redirectToURL);
        }

        if (!IsPostBack)
        {
            //Get logged in id
            int CustID = Convert.ToInt16(Session["CustomerID"]);
            using (eCommerceDBEntities context = new eCommerceDBEntities())
            {
                if (Request.QueryString["addToCart"] != null)
                {
                    int ProdID = Convert.ToInt16(Request.QueryString["addToCart"]);

                    //Check if product is already in cart
                    Cart cr = context.Carts.Where(i => i.ProductID == ProdID).FirstOrDefault();
                    //If not in the DB add it.
                    if (cr == null)
                    {
                        context.Carts.AddObject(new Cart
                        {
                            CustomerID = CustID,
                            ProductID = ProdID,
                            Quantity = 1
                        });
                        context.SaveChanges();
                    }
                }

                var cart = (from c in context.Carts
                            join p in context.Products
                                on c.ProductID equals p.ProductID
                            where c.CustomerID == CustID
                            select new { p.ProductName, p.ProductPrice, p.ProductImageURL, c.CartID });
                Repeater12.DataSource = cart;
                Repeater12.DataBind();

                Boolean isCartEmpty = context.Carts.Where(i => i.CustomerID == CustID).FirstOrDefault() == null;
                //If cart is empty. No ID label means, no cart item
                if (isCartEmpty)
                {
                    Wizard1.Visible = false;
                    lblMessage.Visible = true;
                }
            }
        }
    }

    protected void lnkRemoveItem_Click(object sender, EventArgs e)
    {
        using (eCommerceDBEntities context = new eCommerceDBEntities())
        {
            
            //Get the reference of the clicked button.
            LinkButton button = (sender as LinkButton);
            //Get the Repeater Item reference
            RepeaterItem item = button.NamingContainer as RepeaterItem;
             
            //Get the repeater item index
            int index = item.ItemIndex;
            string id = ((Label)(Repeater12.Items[index].FindControl("lblHiddenCartID"))).Text;
            string id3 = ((DropDownList)(Repeater12.Items[index].FindControl("ddlQuantity"))).SelectedValue;

            int cartid = Convert.ToInt16(id);
            Cart cr = context.Carts.Where(i => i.CartID == cartid).FirstOrDefault();

            context.Carts.DeleteObject(cr);
            context.SaveChanges();

            string notifyTitle = "One item removed";
            string message = "One item was removed from your cart!";
            string notification = string.Format("?notifyTitle={0}&notificationDescription={1}", notifyTitle, message);

            Response.Redirect("~/order.aspx" + notification);
        }
    }
    protected void Wizard1_FinishButtonClick(object sender, WizardNavigationEventArgs e)
    {
       
        Response.Redirect("~/order.aspx");

    }
    protected void btnPay_Click(object sender, EventArgs e)
    {
        DropDownList ctr = (DropDownList)Master.FindControl("ddlQuantity");
        
        using (eCommerceDBEntities context = new eCommerceDBEntities())
        {
            
            int custID = Convert.ToInt16(Session["CustomerID"]);
            if (string.IsNullOrEmpty(txtAmount.Text))
            {
                lblStatus.Text = "Please select mode of payment Debit/Credit card";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                List<Cart> cart = context.Carts.Where(i => i.CustomerID == custID).ToList();

                foreach (var i in cart)
                {
                    //Fill order table
                    context.Orders.AddObject(new Order
                    {
                        CustomerID = custID,
                        ProductID = i.ProductID,
                        DateOrdered = DateTime.Now
                    });

                    //Product is bought so, empty the cart.
                    //context.Carts.DeleteObject(i);
                }
                context.SaveChanges();

                lblStatus.Text = "Your order has been placed. Happy shopping!";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                string notifyTitle = "Payment Successful!";
                string message = "The order has been placed. You will receive your shipment sonn.";
                string notification = string.Format("?notifyTitle={0}&notificationDescription={1}", notifyTitle, message);

               // Response.Redirect("~/order.aspx" + notification);
            }
        }
    }
    protected void rdoDebitCard_CheckedChanged(object sender, EventArgs e)
    {
        addAmount();
        lblStatus.Text = "";
    }

    protected void rdoCredit_CheckedChanged(object sender, EventArgs e)
    {
        addAmount();
        lblStatus.Text = "";
    }
    private void addAmount()
    {
        // btnPay.Text = Convert.ToString(ctr.SelectedValue);
        int CustomerID = Convert.ToInt16(Session["CustomerID"].ToString());
        using (eCommerceDBEntities context = new eCommerceDBEntities())
        {
            var cart = (from c in context.Carts
                        join p in context.Products
                            on c.ProductID equals p.ProductID
                        where c.CustomerID == CustomerID
                        select new { p.ProductPrice, c.Quantity });
            decimal amt = 0;
            foreach (var i in cart)
            {
                amt += (i.ProductPrice * i.Quantity);
            }

            txtAmount.Text = amt.ToString();


        }
    }

    protected void Repeater1_ItemCommand(object source, RepeaterCommandEventArgs e)
    {

    }
    protected void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        int CustID = Convert.ToInt16(Session["CustomerID"]);
        using (eCommerceDBEntities context = new eCommerceDBEntities())
        {
           
           
            //-----------------------------------------------------------
            DropDownList ddlPageSize = (DropDownList)sender;
            RepeaterItem item = ddlPageSize.NamingContainer as RepeaterItem;
            string a = Convert.ToString(ddlPageSize.SelectedValue);

            int index = item.ItemIndex;
            string id = ((Label)(Repeater12.Items[index].FindControl("lblHiddenCartID"))).Text;
            int bb = Convert.ToInt16(id);
            int cc = Convert.ToInt16(a);
            //-----------------------------------------------------------
            
           
             var users = context.Carts.Where(x => (x.CustomerID == CustID)&& (x.CartID==bb));
            
             foreach (var user in users)
             {
                 // change the properties
                 user.Quantity =cc;
                 
             }
            //System.Diagnostics.Debug.WriteLine("gggggggggddddgggggggggggg{0}   {1}", users);
             // EF will pick up those changes and save back to the database.
             context.SaveChanges();
        }
           
    }
    protected void Repeater12_ItemCommand(object source, RepeaterCommandEventArgs e)
    {

    }


    protected void GenerateInvoicePDF(object sender, EventArgs e)
    {
        //Dummy data for Invoice (Bill).
        string companyName = "Mobile Mart";
        int orderNo = 2303;
        int CustID = Convert.ToInt16(Session["CustomerID"]);
        string CustID2 = Convert.ToString(Session["CustomerID"]);
       // eCommerceDBEntities context = new eCommerceDBEntities();

        using (eCommerceDBEntities context = new eCommerceDBEntities())
        {
            //-----------------------------------------------------------

            int CustomerID = Convert.ToInt16(Session["CustomerID"].ToString());
            
                var cart = (from c in context.Carts
                            join p in context.Products
                            on c.ProductID equals p.ProductID
                            
                             
                            where c.CustomerID == CustomerID
                            select new { p.ProductPrice, c.Quantity,p.ProductName ,c.CartID});


                var customer = (from cust in context.Customers
                                where cust.CustomerID == CustomerID
                                select new { cust.CustomerName });



           
           //000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000

            DataTable dt= new DataTable();
            
            

            decimal amt = 0;
            string orderId = "";
            string customerName = "";
            foreach (var i in cart)
            {
                amt += (i.ProductPrice * i.Quantity);
                orderId =Convert.ToString( i.CartID);
            }
            foreach (var i in customer)
            {
                
                customerName = Convert.ToString(i.CustomerName);
            }
              
            //oooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    StringBuilder sb = new StringBuilder();

                    //Generate Invoice (Bill) Header.
                    sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
                    sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>INVOICE</b></td></tr>");
                    sb.Append("<tr><td colspan = '2'></td></tr>");
                    sb.Append("<tr><td><b>Invoice No: </b>");
                    sb.Append(orderId);
                    sb.Append("</td><td align = 'right'><b>Date: </b>");
                    sb.Append(DateTime.Now);
                    sb.Append(" </td></tr>");
                    sb.Append("<tr><td colspan = '2'><b>Company Name: </b>");
                    sb.Append(companyName);
                    sb.Append("</td></tr>");
                    sb.Append("<tr><td colspan = '2'><b>Customer Name: </b>");
                    sb.Append(customerName);
                    sb.Append("</td></tr>");
                    sb.Append("</table>");
                    sb.Append("<br />");

                    //Generate Invoice (Bill) Items Grid.
                    sb.Append("<table border = '1'>");
                    sb.Append("<tr>");
                     
                        sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                        sb.Append("Product Name");
                        sb.Append("</th>");
                        sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                        sb.Append("Quantity");
                        sb.Append("</th>");
                        sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                        sb.Append("Product Price");
                        sb.Append("</th>");
                    
                    sb.Append("</tr>");
                    foreach (var i in cart)
                    {
                        sb.Append("<tr>");
                         
                            sb.Append("<td>");
                            sb.Append(i.ProductName);
                            sb.Append("</td>");
                            sb.Append("<td>");
                            sb.Append(i.Quantity);
                            sb.Append("</td>");
                            sb.Append("<td>");
                            sb.Append(i.ProductPrice);
                            sb.Append("</td>");
                        
                        sb.Append("</tr>");
                    }
                    sb.Append("<tr><td align = 'right' colspan = '");
                    sb.Append(3 - 1);
                    sb.Append("'>Total</td>");
                    sb.Append("<td>");
                    sb.Append(amt.ToString());
                    sb.Append("</td>");
                    sb.Append("</tr></table>");

                    //Export HTML String as PDF.
                    StringReader sr = new StringReader(sb.ToString());
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                    pdfDoc.Open();
                    htmlparser.Parse(sr);
                    pdfDoc.Close();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=Invoice_" + orderNo + ".pdf");
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(pdfDoc);
                    
                    // ====================================================================================
                     int custID = Convert.ToInt16(Session["CustomerID"]);
                    List<Cart> cart2 = context.Carts.Where(ii => ii.CustomerID == custID).ToList();
                    foreach (var i in cart2)
                    {
                        context.Carts.DeleteObject(i);
                    }
                     
                    context.SaveChanges();
                    Response.End();
                }
               
            }
           
        }
       
        //=================================================================================
        Response.Redirect("~/order.aspx");
}
}