using System;
using System.Transactions;
using Rhino.ETL.Engine;

namespace Rhino.ETL.Commands
{
	public class ExecuteInParallelTransactionCommand : ExecuteInParallelCommand
	{
		private TransactionScope scope;

		public ExecuteInParallelTransactionCommand(Target target) : base(target)
		{
			scope = new TransactionScope();
		}

		public ExecuteInParallelTransactionCommand(Target target, IsolationLevel level) : base(target)
		{
			TransactionOptions transactionOptions = new TransactionOptions();
			transactionOptions.IsolationLevel = level;
			scope = new TransactionScope(TransactionScopeOption.Required,
				transactionOptions);
		}

		protected override void RegisterForExecution(ICommand command)
		{
			DependentTransaction dependentTransaction = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
			ExecutionPackage.Current.RegisterForExecution(target, delegate
			{
				using(TransactionScope tx = new TransactionScope(dependentTransaction))
				{
					command.Execute();
					tx.Complete();
				}
			});	
		}

		public override void WaitForCompletion(TimeSpan timeOut)
		{
			base.WaitForCompletion(timeOut);
			scope.Complete();
			scope.Dispose();
		}

		public override void ForceEndOfCompletionWithoutFurtherWait()
		{
			scope.Dispose();
			base.ForceEndOfCompletionWithoutFurtherWait();
		}
	}
}