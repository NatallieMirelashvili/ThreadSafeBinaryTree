public interface IBinaryTree
{
    void Add(string value);
    void Delete(string value);
    int Search(string value);
    void PrintSorted();
}


public class SafeNode
{
    private int Counter;
    private string Data;
    private SafeNode lefty;
    private SafeNode righty;
    private SafeNode parent;

    public SafeNode(string data)
    {
        this.Data = data;
        Counter = 1;
    }

    public int getCount()
    {
        return Counter;
    }
    public string getData()
    {
        return Data;
    }
    public SafeNode getLefty()
    {
        return lefty;
    }
    public SafeNode getRighty()
    {
        return righty;
    }
    public SafeNode getParent()
    {
        return parent;
    }
    public void setLefty(SafeNode lefty)
    {
        this.lefty = lefty;
    }
    public void setRighty(SafeNode righty)
    {
        this.righty = righty;
    }
    public void setParent(SafeNode parent)
    {
        this.parent = parent;
    }
    public void addOne()
    {
        Counter += 1;
    }
    public void removeOne()
    {
        Counter -= 1;
    }

    public string PrintNode()
    {
        return $"{Data} ({Counter})";
    }
}




public class ThreadSafeBinaryTree : IBinaryTree
{
    private SafeNode root;
    Mutex WritersLock = new Mutex();
    Mutex ReadersLock = new Mutex();
    private int CountReaders;


    public void Add(string value)
    {
        WritersLock.WaitOne();
        try
        {

            AddRec(root, value);
        }
        finally
        {
            WritersLock.ReleaseMutex();
        }
    }

    public void Delete(string value)
    {
        WritersLock.WaitOne();
        try
        {
            DelOrDec(root, value);
        }
        finally
        {
            WritersLock.ReleaseMutex();
        }


    }

    public int Search(string value)
    {
        ReadersLock.WaitOne();
        CountReaders++;
        if (CountReaders == 1)
        {
            WritersLock.WaitOne();
        }
        ReadersLock.ReleaseMutex();

        try
        {
            return SearchRec(root, value);
        }
        finally
        {
            ReadersLock.WaitOne();
            CountReaders--;
            if (CountReaders == 0)
                WritersLock.ReleaseMutex();
            ReadersLock.ReleaseMutex();
        }

    }
    public void PrintSorted()
    {
        ReadersLock.WaitOne();
        CountReaders++;
        if (CountReaders == 1)
            WritersLock.WaitOne();
        ReadersLock.ReleaseMutex();
        try
        {
            PrintInorder(root);
        }
        finally
        {
            ReadersLock.WaitOne();
            CountReaders--;
            if (CountReaders == 0)
                WritersLock.ReleaseMutex();
            ReadersLock.ReleaseMutex();
        }
    }





    private void AddRec(SafeNode currentNode, string value)
    {
        if (root == null)
        {
            SafeNode newNode = new(value);
            root = newNode;
            return;
        }
        int comparison = string.Compare(currentNode.getData(), value);

        if (comparison == 0)
        {
            currentNode.addOne();
            return;
        }
        if (comparison > 0)
        {
            if (currentNode.getLefty() == null)
            {
                SafeNode newNode = new SafeNode(value);
                currentNode.setLefty(newNode);
                newNode.setParent(currentNode);
            }
            else
            {
                AddRec(currentNode.getLefty(), value);
            }
        }
        else
        {
            if (currentNode.getRighty() == null)
            {
                SafeNode newNode = new SafeNode(value);
                currentNode.setRighty(newNode);
                newNode.setParent(currentNode);
            }
            else
            {
                AddRec(currentNode.getRighty(), value);
            }
        }
    }


    private void DelOrDec(SafeNode currentNode, string value)
    {
        int comparison = string.Compare(currentNode.getData(), value);
        if (comparison == 0)
        {
            if (currentNode.getCount() - 1 == 0)
            {
                DeleteCases(currentNode);
                return;
            }
            currentNode.removeOne();
            return;
        }
        if (comparison > 0)
        {
            DelOrDec(currentNode.getLefty(), value);
            return;
        }
        DelOrDec(currentNode.getRighty(), value);

    }

    private void transplate(SafeNode oldNode, SafeNode newNode)
    {
        if (oldNode == null) { return; }
        //Node to delete is the root itself:
        if (oldNode.getParent() == null)
        {
            root = newNode;
        }
        //Node to delete is the left son of its parent:
        else if (oldNode == oldNode.getParent().getLefty())
        {
            oldNode.getParent().setLefty(newNode);
        }
        //Node to delete is the right son of its parent:
        else
        {
            oldNode.getParent().setRighty(newNode);
        }
        //update new son Node's parent:
        if (newNode != null)
        {
            newNode.setParent(oldNode.getParent());
        }

    }

    private SafeNode findSuccessive(SafeNode node)
    {
        if (node == null)
            return null;

        while (node.getLefty() != null)
        {
            node = node.getLefty();
        }
        return node;

    }

    private void DeleteCases(SafeNode node)
    {
        if (node == null) return;  // Node to delete should not be null

        if (node.getLefty() == null)
        {
            // Node to delete has only a right child
            transplate(node, node.getRighty());
        }
        else if (node.getRighty() == null)
        {
            // Node to delete has only a left child
            transplate(node, node.getLefty());
        }
        else
        {
            // Node to delete has two children
            SafeNode successive = findSuccessive(node.getRighty());

            if (successive != null && successive.getParent() != node)
            {
                // Successive is not the immediate right child
                transplate(successive, successive.getRighty());
                successive.setRighty(node.getRighty());

                if (successive.getRighty() != null)
                {
                    successive.getRighty().setParent(successive);
                }
            }

            transplate(node, successive);
            if (successive != null)
            {
                successive.setLefty(node.getLefty());

                if (successive.getLefty() != null)
                {
                    successive.getLefty().setParent(successive);
                }
            }
        }
    }


    private int SearchRec(SafeNode current, string value)
    {
        if (current == null)
        {
            return 0;
        }
        int comparison = string.Compare(current.getData(), value);
        if (comparison == 0)
        {
            return current.getCount();
        }
        if (comparison > 0)
        {
            return SearchRec(current.getLefty(), value);
        }
        return SearchRec(current.getRighty(), value);

    }


    private void PrintInorder(SafeNode current)
    {
        if (current == null)
        {
            return;
        }
        PrintInorder(current.getLefty());
        Console.WriteLine($"{current.PrintNode()}\n\n");
        PrintInorder(current.getRighty());

    }



}


