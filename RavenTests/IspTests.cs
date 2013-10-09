namespace IspTests
{
	using NUnit.Framework;
	using Moq;

	[TestFixture]
	public class CustomerServiceTests
	{
		[Test]
		public void CanGetCustomerById()
		{
			var repository = new Mock<IRepository<Customer>>();
			repository.Setup(x => x.GetById(It.IsAny<int>())).Returns(new Customer());
		   
			var sut = new CustomerService(repository.Object, null);
			var customer = sut.GetCustomer(1);
			Assert.NotNull(customer);
		}

		[Test]
		public void CreatingCustomerSendsWelcomeEmail()
		{
			var email = new Mock<IEmailService>();
			var sut = new CustomerService(new Mock<IRepository<Customer>>().Object, email.Object);
			sut.CreateCustomer(new Customer());
			email.Verify(x => x.SendWelcomeEmail(It.IsAny<Customer>()), Times.Once);
		}
	}

	public interface ICustomerService
	{
		Customer GetCustomer(int id);
		void CreateCustomer(Customer customer);
	}

	public interface IRepository<T>
	{
		T GetById(int id);
		void Add(T entity);
	}

	public interface IEmailService
	{
		void SendWelcomeEmail(Customer customer);
		void SendDailyAppStatusToOperations(Customer customer);
	}

	public class Customer
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	
	public class CustomerService : ICustomerService
	{
		private readonly IRepository<Customer> _repository;
		private readonly IEmailService _email;

		public CustomerService(IRepository<Customer> repository, IEmailService email)
		{
			_repository = repository;
			_email = email;
		}

		public Customer GetCustomer(int id)
		{
			return _repository.GetById(id);
		}

		public void CreateCustomer(Customer customer)
		{
			_repository.Add(customer);
			_email.SendWelcomeEmail(customer);
		}
	}
}