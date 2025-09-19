using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Linq;

public class CypherExpressionTranslator : ExpressionVisitor
{
	private readonly StringBuilder _queryBuilder = new StringBuilder();
	private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
	private int _parameterIndex;

	public (string Query, IReadOnlyDictionary<string, object> Parameters) Translate(Expression expression)
	{
		 _queryBuilder.Clear();
		_parameters.Clear();
		_parameterIndex = 0;

		Visit(expression);

		return (_queryBuilder.ToString(), _parameters);
	}

	protected override Expression VisitMethodCall(MethodCallExpression node)
	{
		if (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
		{
			switch (node.Method.Name)
			{
				case "Where":
					VisitWhere(node);
					break;
				case "Select":
					VisitSelect(node);
					break;
				case "FirstOrDefault":
				case "First":
				case "SingleOrDefault":
				case "Single":
					VisitSingleResultMethod(node);
					break;
				case "OrderBy":
				case "OrderByDescending":
					VisitOrderBy(node);
					break;
				case "ThenBy":
				case "ThenByDescending":
					VisitThenBy(node);
					break;
				default:
					throw new NotSupportedException($"Method {node.Method.Name} is not supported");
			}
		}
		else
		{
			throw new NotSupportedException("Only Queryable methods are supported");
		}

		return node;
	}

	private void VisitWhere(MethodCallExpression node)
	{
		// Обрабатываем предикат WHERE
		var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
		_queryBuilder.Append("MATCH (n) WHERE ");
		Visit(lambda.Body);
	}

	private void VisitSelect(MethodCallExpression node)
	{
		// Обрабатываем проекцию SELECT
		var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
		_queryBuilder.Append("RETURN ");
		Visit(lambda.Body);
	}

	private void VisitSingleResultMethod(MethodCallExpression node)
	{
		// Обрабатываем методы, возвращающие один результат
		Visit(node.Arguments[0]);
		_queryBuilder.Append(" LIMIT 1");
	}

	private void VisitOrderBy(MethodCallExpression node)
	{
		// Обрабатываем сортировку ORDER BY
		Visit(node.Arguments[0]);
		var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
		_queryBuilder.Append(" ORDER BY ");
		Visit(lambda.Body);
		_queryBuilder.Append(node.Method.Name.EndsWith("Descending") ? " DESC" : " ASC");
	}

	private void VisitThenBy(MethodCallExpression node)
	{
		// Обрабатываем дополнительную сортировку
		var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
		_queryBuilder.Append(", ");
		Visit(lambda.Body);
		_queryBuilder.Append(node.Method.Name.EndsWith("Descending") ? " DESC" : " ASC");
	}

	protected override Expression VisitBinary(BinaryExpression node)
	{
		Visit(node.Left);

		switch (node.NodeType)
		{
			case ExpressionType.Equal:
				_queryBuilder.Append(" = ");
				break;
			case ExpressionType.NotEqual:
				_queryBuilder.Append(" <> ");
				break;
			case ExpressionType.GreaterThan:
				_queryBuilder.Append(" > ");
				break;
			case ExpressionType.GreaterThanOrEqual:
				_queryBuilder.Append(" >= ");
				break;
			case ExpressionType.LessThan:
				_queryBuilder.Append(" < ");
				break;
			case ExpressionType.LessThanOrEqual:
				_queryBuilder.Append(" <= ");
				break;
			case ExpressionType.AndAlso:
				_queryBuilder.Append(" AND ");
				break;
			case ExpressionType.OrElse:
				_queryBuilder.Append(" OR ");
				break;
			default:
				throw new NotSupportedException($"Binary operator {node.NodeType} is not supported");
		}

		Visit(node.Right);
		return node;
	}

	protected override Expression VisitMember(MemberExpression node)
	{
		if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
		{
			_queryBuilder.Append($"n.{node.Member.Name}");
		}
		else
		{
			throw new NotSupportedException("Complex member expressions are not supported");
		}

		return node;
	}

	protected override Expression VisitConstant(ConstantExpression node)
	{
		var parameterName = $"p{_parameterIndex++}";
		_parameters[parameterName] = node.Value;
		_queryBuilder.Append($"${parameterName}");
		return node;
	}

	private static Expression StripQuotes(Expression expression)
	{
		while (expression.NodeType == ExpressionType.Quote)
		{
			expression = ((UnaryExpression)expression).Operand;
		}
		return expression;
	}
}