using Domain.Models;
using EntInvoicing.Domain.Persistence;
using EntInvoicing.WebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment3_4Api.Controllers
{
    public class InvoicingController : Controller
    {
        /// <summary>
        /// The invoice repository needed by the invoice binder which is cached in the Cache state of the
        /// web application per the requirements
        /// </summary>
        private IInvoiceRepository _invoiceRepository;

        /// <summary>
        /// Instance constructor called whenever a request is being processed. The invoice repository
        /// is being resolved through dependency injection
        /// </summary>
        /// <param name="invoiceBinder"></param>
        public InvoicingController(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// HTTP Get response to a request to add an invoice. Delivers the empty Invoice Form to the user
        /// so they can add an invoice.
        /// </summary>
        /// <returns>the InvoiceForm View</returns>
        [HttpGet]
        public ActionResult AddInvoice()
        {
            ViewBag.InvoiceFormScope = InvoiceFormScope.Add;

            //ensure an empty invoice model object is passed  which has the ID property set to a default. 
            //withouth it the the framework will assume it to be an error that the ID was not set. The ID
            //cannot be set by the user since it is only displayed as a hidden field.
            return View("InvoiceForm", new InvoiceViewModel());
        }

        /// <summary>
        /// HTTP Post response to the request to add an invoice. Triggered when the user submits an actual invoice
        /// to be added to the system. The method will add the invoice to the invoice binder and will allow the
        /// user to add the next invoice
        /// </summary>
        /// <param name="newInvoice"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddInvoice(InvoiceViewModel vmInvoice)
        {
            if (ModelState.IsValid)
            {
                //add the invoice to the invoice binder
                Invoice invoice = vmInvoice.Invoice;
                this.InvoiceBinder.AddInvoice(invoice);

                //allow the user to add the next invoice
                return RedirectToAction("AddInvoice");
            }
            else
            {
                //The invoice input data has errors, display and allow the user to resubmit
                ViewBag.InvoiceFormScope = InvoiceFormScope.Add;
                return View("InvoiceForm", vmInvoice);
            }
        }

        /// <summary>
        /// Action method that is available only for manager users and allows them to see the list of
        /// receivables and to manage them 
        /// </summary>
        /// <returns></returns>
        public ActionResult Receivables()
        {
            //check whether the current user is eligible for this operation. If not send the user to the login page
            if (AppUser.Role != UserRole.Manager)
            {
                TempData[EntInvoicingApplication.TEMP_OBJ_ERRMSG] = $"Unauthorized access by {AppUser.UserName} . The user is not a manager.";
                return RedirectToAction("Login", "UserAccounts");
            }

            return View("Receivables", this.InvoiceBinder);
        }

        [HttpGet]
        public ActionResult EditInvoice(int invoiceId)
        {
            //check whether the current user is eligible for this operation. If not send the user to the login page
            if (AppUser.Role != UserRole.Manager)
            {
                TempData[EntInvoicingApplication.TEMP_OBJ_ERRMSG] = $"Unauthorized access by {AppUser.UserName} . The user is not a manager.";
                return RedirectToAction("Login", "UserAccounts");
            }

            //access the invoice with the given ID from the invoice binder
            Invoice invoice = this.InvoiceBinder.GetInvoice(invoiceId);

            //display the invoice form for the given invoice in the edit form
            ViewBag.InvoiceFormScope = InvoiceFormScope.Edit;
            return View("InvoiceForm", new InvoiceViewModel(invoice));
        }

        [HttpPost]
        public ActionResult EditInvoice(InvoiceViewModel vmInvoice)
        {
            //check whether the current user is eligible for this operation. If not send the user to the login page
            if (AppUser.Role != UserRole.Manager)
            {
                TempData[EntInvoicingApplication.TEMP_OBJ_ERRMSG] = $"Unauthorized access by {AppUser.UserName} . The user is not a manager.";
                return RedirectToAction("Login", "UserAccounts");
            }

            //check the data received to make sure it is represents a valid invoice
            if (ModelState.IsValid)
            {
                //ask the repository to update the invoice represented by the invoice view model
                this.InvoiceBinder.UpdateInvoice(vmInvoice.Invoice);

                //display the receivables view
                return View("Receivables", this.InvoiceBinder);
            }
            else
            {
                //the invoice is not valid, allow the user to make further edits
                ViewBag.InvoiceFormScope = InvoiceFormScope.Edit;
                return View("EditInvoice", vmInvoice);
            }
        }

        [HttpPost]
        public ActionResult InvoicePaid(int invoiceId)
        {
            //check whether the current user is eligible for this operation. If not send the user to the login page
            if (AppUser.Role != UserRole.Manager)
            {
                TempData[EntInvoicingApplication.TEMP_OBJ_ERRMSG] = $"Unauthorized access by {AppUser.UserName} . The user is not a manager.";
                return RedirectToAction("Login", "UserAccounts");
            }

            //retrieve the invoice from the invoice binder
            this.InvoiceBinder.PayInvoice(invoiceId);

            //display the receivables page which should no longer include the given invoice
            return RedirectToAction("Receivables");
        }

        /// <summary>
        /// Provides access to the invoice binder using the cache state. If the information is not there
        /// the invoice binder will be created and stored in the cache for further access.
        /// </summary>
        private InvoiceBinder InvoiceBinder
        {
            get
            {
                //check if it exists in the cache and create and cache it if not
                InvoiceBinder invBinder = HttpContext.Cache["InvoiceBinderCache"] as InvoiceBinder;
                if (invBinder == null)
                {
                    //this is the first type the binder is being accessed or the cached information has timed out
                    //create an invoice binder and store it in the cache state
                    invBinder = new InvoiceBinder(_invoiceRepository);
                    HttpContext.Cache[EntInvoicingApplication.CACHE_OBJ_INVBINDER] = invBinder;
                }

                //return the invoice binder from the cache
                return invBinder;
            }
        }

        /// <summary>
        /// Implementation property to simplify the code to access the user. Named AppUser instead of User because
        /// MVC and the Controller base class already has a User property which is part of the mechanism we are implementing
        /// in this assignment manually. In other words when implementing authentication using the MVC mechanisms this would
        /// not be needed.
        /// </summary>
        private User AppUser
        {
            get { return Session[EntInvoicingApplication.SESSION_OBJ_USER] as User; }
        }
        // GET: Invoicing
    }
}