using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yort.Zip.InStore.Tests
{
	[TestClass]
	public class RequestValidateTests
	{

		#region CreateOrderRequest Validation

		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresMechantReference()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
				Order = new ZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA05",
					MerchantReference = null,
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Order.MerchantReference = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Order.MerchantReference = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresTerminalId()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = null,
				Order = new ZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA05",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.TerminalId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.TerminalId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresApprovalCode()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
				Order = new ZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = null,
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Order.CustomerApprovalCode = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Order.CustomerApprovalCode = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresOperator()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
				Order = new ZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA01",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = null,
					PaymentFlow = ZipPaymentFlow.Payment,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Order.Operator = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Order.Operator = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresOrder()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresPaymentFlow()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
				Order = new ZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA01",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = null,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Order.PaymentFlow = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Order.PaymentFlow = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}


		[TestMethod]
		public void Validate_CreateOrderRequest_RequiresPositiveAmount()
		{
			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
				Order = new ZipOrder()
				{
					Amount = 0M,
					CustomerApprovalCode = "AA01",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => request.Validate());

			request.Order.Amount = -10.5M;
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => request.Validate());
		}

		#endregion

		#region RefundOrderRequest Validation

		[TestMethod]
		public void Validate_RefundOrderRequest_RequiresMechantReference()
		{
			var request = new RefundOrderRequest() 
			{ 
				MerchantRefundReference = null, 
				OrderId = System.Guid.NewGuid().ToString(), 
				Amount = 10.5M, 
				Operator = "Test", 
				TerminalId = "2531" 
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.MerchantRefundReference = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.MerchantRefundReference = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_RefundOrderRequest_OrderId()
		{
			var request = new RefundOrderRequest()
			{
				MerchantRefundReference = System.Guid.NewGuid().ToString(),
				OrderId = null,
				Amount = 10.5M,
				Operator = "Test",
				TerminalId = "2531"
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_RefundOrderRequest_Operator()
		{
			var request = new RefundOrderRequest()
			{
				MerchantRefundReference = System.Guid.NewGuid().ToString(),
				OrderId = System.Guid.NewGuid().ToString(),
				Amount = 10.5M,
				Operator = null,
				TerminalId = "2531"
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_RefundOrderRequest_TerminalId()
		{
			var request = new RefundOrderRequest()
			{
				MerchantRefundReference = System.Guid.NewGuid().ToString(),
				OrderId = System.Guid.NewGuid().ToString(),
				Amount = 10.5M,
				Operator = "Test",
				TerminalId = null
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_RefundOrderRequest_PositiveAmount()
		{
			var request = new RefundOrderRequest()
			{
				MerchantRefundReference = System.Guid.NewGuid().ToString(),
				OrderId = System.Guid.NewGuid().ToString(),
				Amount = 0M,
				Operator = "Test",
				TerminalId = "2531"
			};

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => request.Validate());

			request.Amount = -10.5M;
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => request.Validate());
		}

		#endregion

		#region EnrolRequest

		[TestMethod]
		public void Validate_EnrolRequest_RequiresActivationCode()
		{
			var request = new EnrolRequest()
			{
				ActivationCode = null,
				Secret = "123",
				Terminal = "2531"
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.ActivationCode = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.ActivationCode = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_EnrolRequest_RequiresSecret()
		{
			var request = new EnrolRequest()
			{
				ActivationCode = "ABC",
				Secret = null,
				Terminal = "2531"
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Secret = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Secret = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_EnrolRequest_RequiresTerminal()
		{
			var request = new EnrolRequest()
			{
				ActivationCode = "ABC",
				Secret = "123",
				Terminal = null
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Terminal = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Terminal = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		#endregion

		#region OrderStatusRequest Validation

		[TestMethod]
		public void Validate_EnrolRequest_RequiresOrderId()
		{
			var request = new OrderStatusRequest()
			{
				OrderId = null
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		#endregion

		#region CancelOrderRequest Validation

		[TestMethod]
		public void Validate_CancelOrderRequest_RequiresOrderId()
		{
			var request = new CancelOrderRequest()
			{
				Operator = "Test",
				OrderId = null,
				TerminalId = "2531"
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CancelOrderRequest_RequiresOperator()
		{
			var request = new CancelOrderRequest()
			{
				Operator = null,
				OrderId = System.Guid.NewGuid().ToString(),
				TerminalId = "2531"
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.Operator = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.Operator = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		[TestMethod]
		public void Validate_CancelOrderRequest_RequiresTerminalId()
		{
			var request = new CancelOrderRequest()
			{
				Operator = "Test",
				OrderId = System.Guid.NewGuid().ToString(),
				TerminalId = null
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.TerminalId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.TerminalId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		#endregion

		#region RollbackOrderRequest Validation

		[TestMethod]
		public void Validate_RollbackOrderRequest_RequiresOrderId()
		{
			var request = new RollbackOrderRequest()
			{
				OrderId = null
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		#endregion

		#region CommitOrderRequest Validation

		[TestMethod]
		public void Validate_CommitOrderRequest_RequiresOrderId()
		{
			var request = new CommitOrderRequest()
			{
				OrderId = null
			};

			Assert.ThrowsException<ArgumentNullException>(() => request.Validate());

			request.OrderId = String.Empty;
			Assert.ThrowsException<ArgumentException>(() => request.Validate());

			request.OrderId = "   ";
			Assert.ThrowsException<ArgumentException>(() => request.Validate());
		}

		#endregion


	}
}
