package at.playground;

import at.playground.history.NoticeHistory;
import at.playground.history.PersonHistory;
import org.hibernate.Session;

import javax.persistence.EntityManager;
import javax.persistence.EntityManagerFactory;
import javax.persistence.Persistence;
import java.time.Instant;
import java.util.*;

public class Playground {
    public static void main(String[] args) {
        Instant timeOfInsert = Instant.now();
        Instant timeOfUpdate = timeOfInsert.plusSeconds(2);

        /********** INSERT **********/
        EntityManagerFactory emf = Persistence.createEntityManagerFactory("Playground");
        EntityManager entityManagerCreate = emf.createEntityManager();

        entityManagerCreate.getTransaction().begin();

        Request request = new Request();
        request.setRequestTime(timeOfInsert);

        Person person = new Person();

        person.setFirstName("Michael");
        person.setLastName("Vodep");
        person.setChangedByRequest(request);

        Notice notice1 = new Notice();
        notice1.setNotice("Notiz 1");
        notice1.setPerson(person);
        notice1.setChangedByRequest(request);

        Notice notice2 = new Notice();
        notice2.setNotice("Notiz 2");
        notice2.setPerson(person);
        notice2.setChangedByRequest(request);

        person.setNotices(Arrays.asList(notice1, notice2));

        entityManagerCreate.persist(person);

        entityManagerCreate.getTransaction().commit();
        entityManagerCreate.close();

        /********** UPDATE **********/
        EntityManager entityManagerUpdate = emf.createEntityManager();
        entityManagerUpdate.getTransaction().begin();

        Request requestForUpdate = new Request();
        requestForUpdate.setRequestTime(timeOfUpdate);

        Person personForUpdate = entityManagerUpdate.find(Person.class, 1L);

        personForUpdate.setFirstName(personForUpdate.getFirstName() + " update");
        personForUpdate.setLastName(personForUpdate.getLastName() + " update");
        personForUpdate.setChangedByRequest(requestForUpdate);

        Notice notice =  personForUpdate.getNotices().get(0);

        notice.setNotice(notice.getNotice() + " Update");
        notice.setChangedByRequest(requestForUpdate);

        notice =  personForUpdate.getNotices().get(1);

        notice.setNotice(notice.getNotice() + " Update");
        notice.setChangedByRequest(requestForUpdate);

        Notice newNotice = new Notice();
        newNotice.setNotice("New Notice");
        newNotice.setChangedByRequest(requestForUpdate);
        newNotice.setPerson(personForUpdate);

        personForUpdate.getNotices().add(newNotice);

        entityManagerUpdate.merge(personForUpdate);

        entityManagerUpdate.getTransaction().commit();
        entityManagerUpdate.close();

        /********** HISTORY BEFORE UPDATE **********/
        EntityManager entityManagerHistory = emf.createEntityManager();

        entityManagerHistory.unwrap(Session.class).enableFilter("HistoryFilter").setParameter("filterDate", timeOfInsert);

        PrintPersonToConsole("BEFORE UPDATE", entityManagerHistory.createQuery("select a from PersonHistory a", PersonHistory.class).getResultList());

        entityManagerHistory.close();

        /********** HISTORY AFTER UPDATE **********/

        EntityManager entityManagerHistoryAfterUpdate = emf.createEntityManager();

        entityManagerHistoryAfterUpdate.unwrap(Session.class).enableFilter("HistoryFilter").setParameter("filterDate", timeOfUpdate);

        PrintPersonToConsole("AFTER UPDATE", entityManagerHistoryAfterUpdate.createQuery("select a from PersonHistory a", PersonHistory.class).getResultList());

        entityManagerHistoryAfterUpdate.close();

        emf.close();
    }

    private static void PrintPersonToConsole(String description, List<PersonHistory> persons) {
        System.out.println(description);

        for (PersonHistory personHistory : persons)
        {
            System.out.println(personHistory.getFirstName());
            System.out.println(personHistory.getLastName());

            for (NoticeHistory noticeHistory : personHistory.getNotices())
            {
                System.out.println(noticeHistory.getNotice());
            }
        }
    }
}
